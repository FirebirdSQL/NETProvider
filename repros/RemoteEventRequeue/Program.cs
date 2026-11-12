using System;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;

namespace RemoteEventRequeueRepro
{
    internal static class Program
    {
        private const string EventName = "NETPROVIDER_EVENT_REPRO";

        private static int Main(string[] args)
        {
            string rawConnectionString = Environment.GetEnvironmentVariable("FB_EVENT_REPRO_CONNECTION");
            if (string.IsNullOrWhiteSpace(rawConnectionString))
            {
                Console.Error.WriteLine("FB_EVENT_REPRO_CONNECTION is missing. Use only an isolated test database.");
                return 2;
            }

            int iterations = ReadPositiveInteger(args, 0, 1000);
            int holdMilliseconds = ReadPositiveInteger(args, 1, 10);
            var builder = new FbConnectionStringBuilder(rawConnectionString) { Pooling = false };
            Console.WriteLine("Target=" + builder.DataSource + ":" + builder.Port +
                ", Database=" + builder.Database + ", ServerType=" + builder.ServerType);
            string connectionString = builder.ToString();
            using (var connectionProbe = new FbConnection(connectionString))
            {
                Console.WriteLine("Connection probe: Open");
                connectionProbe.Open();
                Console.WriteLine("Connection probe: connected");
            }
            int published = 0;
            int received = 0;
            int queueErrors = 0;
            int cleanupErrors = 0;
            int eventErrors = 0;
            int completed = 0;

            using (var stop = new CancellationTokenSource())
            using (var publisherReady = new ManualResetEventSlim(false))
            {
                Task publisher = Task.Factory.StartNew(
                    () => PublishEvents(connectionString, publisherReady, stop.Token,
                        () => Interlocked.Increment(ref published)),
                    CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                Console.WriteLine("Waiting for publisher connection.");
                if (!publisherReady.Wait(TimeSpan.FromSeconds(20)))
                {
                    Console.Error.WriteLine("Publisher connection not ready after 20 seconds.");
                    return 2;
                }
                if (publisher.IsFaulted)
                {
                    Console.Error.WriteLine("Publisher failed to start: " + publisher.Exception.GetBaseException());
                    return 2;
                }

                for (int i = 0; i < iterations; i++)
                {
                    FbRemoteEvent events = null;
                    bool queued = false;
                    try
                    {
                        events = new FbRemoteEvent(connectionString);
                        events.RemoteEventCounts += (sender, e) => Interlocked.Increment(ref received);
                        events.RemoteEventError += (sender, e) =>
                        {
                            Interlocked.Increment(ref eventErrors);
                            Console.Error.WriteLine("Event error: " + e.Error);
                        };
                        Console.WriteLine("Iteration " + (i + 1) + ": Open");
                        events.Open();
                        Console.WriteLine("Iteration " + (i + 1) + ": QueueEvents");
                        events.QueueEvents(new[] { EventName });
                        Console.WriteLine("Iteration " + (i + 1) + ": registered");
                        queued = true;
                        Thread.Sleep(holdMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        queueErrors++;
                        Console.Error.WriteLine("Iteration " + (i + 1) + ", Open/QueueEvents: " + ex);
                    }
                    finally
                    {
                        if (events != null)
                        {
                            if (queued)
                            {
                                try { events.CancelEvents(); }
                                catch (Exception ex)
                                {
                                    cleanupErrors++;
                                    Console.Error.WriteLine("Iteration " + (i + 1) + ", CancelEvents: " + ex);
                                }
                            }
                            try { events.Dispose(); }
                            catch (Exception ex)
                            {
                                cleanupErrors++;
                                Console.Error.WriteLine("Iteration " + (i + 1) + ", Dispose: " + ex);
                            }
                        }
                    }
                    completed++;

                    if (publisher.IsFaulted)
                        break;
                }

                stop.Cancel();
                try { publisher.GetAwaiter().GetResult(); }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Publisher error: " + ex);
                    return 2;
                }
            }

            Console.WriteLine("Provider=" + typeof(FbRemoteEvent).Assembly.GetName().Version +
                ", Iterations=" + completed + ", sent=" + published +
                ", received=" + received + ", Queue errors=" + queueErrors +
                ", Event errors=" + eventErrors + ", Cleanup errors=" + cleanupErrors);
            return queueErrors == 0 && eventErrors == 0 && cleanupErrors == 0 ? 0 : 1;
        }

        private static void PublishEvents(string connectionString, ManualResetEventSlim ready,
            CancellationToken stop, Action published)
        {
            try
            {
                Console.WriteLine("Publisher: create connection");
                using (var connection = new FbConnection(connectionString))
                {
                    Console.WriteLine("Publisher: Open");
                    connection.Open();
                    Console.WriteLine("Publisher: connected");
                    ready.Set();
                    while (!stop.IsCancellationRequested)
                    {
                        using (var transaction = connection.BeginTransaction())
                        using (var command = new FbCommand())
                        {
                            command.Connection = connection;
                            command.Transaction = transaction;
                            command.CommandText = "EXECUTE BLOCK AS BEGIN POST_EVENT '" + EventName + "'; END";
                            command.ExecuteNonQuery();
                            transaction.Commit();
                        }
                        published();
                        Thread.Sleep(1);
                    }
                }
            }
            finally
            {
                ready.Set();
            }
        }

        private static int ReadPositiveInteger(string[] args, int index, int defaultValue)
        {
            int result;
            return args.Length > index && int.TryParse(args[index], out result) && result > 0
                ? result : defaultValue;
        }
    }
}
