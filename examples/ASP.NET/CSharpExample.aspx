<%@ Page Language="C#" %>
<%@ import Namespace="System.Data" %>
<%@ import Namespace="FirebirdSql.Data.Firebird" %>
<script runat="server">

    string          myConnectionString;
    FbConnection    myConnection;
    FbTransaction   myTransaction;
    string          commandText;
    
    public void Connection()
    {
        myConnectionString	=
                "User=SYSDBA;"                  +
                "Password=masterkey;"           +
                "Database=SampleDatabase.fdb;"  +
                "DataSource=localhost;"         +
                "Port=3050;"                    +
                "Dialect=3;"                    +
                "Charset=ISO8859_1;"            +
                "Role=;"                        +
                "Connection lifetime=30;"       +
                "Pooling=true;"                 +
                "Packet Size=8192";
    
        myConnection = new FbConnection(myConnectionString);
        myConnection.Open();
    
        myTransaction = myConnection.BeginTransaction();
    
        lblStatus.Text = "Connection succeful";
    
        myConnection.Close();
    }
    
    void cmdConnect_Click(Object sender, EventArgs e) {
        Connection();
    }

</script>
<html>
<head>
</head>
<body>
    <form runat="server">
        <asp:Button id="cmdConnect" onclick="cmdConnect_Click" runat="server" Text="Connect"></asp:Button>
        <asp:Label id="lblStatus" runat="server" font-bold="True" font-names="Verdana" font-size="X-Small">Click
        in button for made connection to Firebird</asp:Label>
    </form>
</body>
</html>
