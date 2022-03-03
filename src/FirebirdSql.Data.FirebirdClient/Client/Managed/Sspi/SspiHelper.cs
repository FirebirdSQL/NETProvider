/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Vladimir Bodecek, Nathan Fox, Jiri Cincura (jiri@cincura.net)

// Adapted from pinvoke.net

using System;
using System.Runtime.InteropServices;

namespace FirebirdSql.Data.Client.Managed.Sspi;

internal sealed class SspiHelper : IDisposable
{
	public string Name { get; } = "Win_Sspi";

	private const int SECBUFFER_VERSION = 0;

	private enum SecBufferType
	{
		SECBUFFER_EMPTY = 0,
		SECBUFFER_DATA = 1,
		SECBUFFER_TOKEN = 2,
	}

	#region Structures used in native Win API calls

	[StructLayout(LayoutKind.Sequential)]
	public struct SecHandle
	{
		public IntPtr LowPart;
		public IntPtr HighPart;

		public SecHandle(int? dummy = null)
		{
			LowPart = IntPtr.Zero;
			HighPart = IntPtr.Zero;
		}

		public bool IsInvalid
		{
			get { return LowPart == IntPtr.Zero && HighPart == IntPtr.Zero; }
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SecInteger
	{
		public uint LowPart;
		public int HighPart;

		public SecInteger(int? dummy = null)
		{
			LowPart = 0;
			HighPart = 0;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct SecBuffer : IDisposable
	{
		private int cbBuffer;
		private int bufferType;
		private IntPtr pvBuffer;

		public SecBuffer(int bufferSize)
		{
			cbBuffer = bufferSize;
			bufferType = (int)SecBufferType.SECBUFFER_TOKEN;
			pvBuffer = Marshal.AllocHGlobal(bufferSize);
		}

		public SecBuffer(byte[] secBufferBytes)
			: this(secBufferBytes.Length)
		{
			Marshal.Copy(secBufferBytes, 0, pvBuffer, cbBuffer);
		}

		public SecBuffer(byte[] secBufferBytes, SecBufferType bufferType)
			: this(secBufferBytes)
		{
			this.bufferType = (int)bufferType;
		}

		public void Dispose()
		{
			if (pvBuffer != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(pvBuffer);
				pvBuffer = IntPtr.Zero;
			}
		}

		public byte[] GetBytes()
		{
			byte[] buffer = null;
			if (cbBuffer > 0)
			{
				buffer = new byte[cbBuffer];
				Marshal.Copy(pvBuffer, buffer, 0, cbBuffer);
			}
			return buffer;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct SecBufferDesc : IDisposable
	{
		public int ulVersion;
		public int cBuffers;
		public IntPtr pBuffers;

		public SecBufferDesc(int bufferSize)
		{
			ulVersion = SECBUFFER_VERSION;
			cBuffers = 1;
			var secBuffer = new SecBuffer(bufferSize);
			pBuffers = Marshal.AllocHGlobal(Marshal.SizeOf(secBuffer));
			Marshal.StructureToPtr(secBuffer, pBuffers, false);
		}

		public SecBufferDesc(byte[] secBufferBytes)
		{
			ulVersion = SECBUFFER_VERSION;
			cBuffers = 1;
			var secBuffer = new SecBuffer(secBufferBytes);
			pBuffers = Marshal.AllocHGlobal(Marshal.SizeOf(secBuffer));
			Marshal.StructureToPtr(secBuffer, pBuffers, false);
		}

		public void Dispose()
		{
			if (pBuffers != IntPtr.Zero)
			{
				var secBuffer = Marshal.PtrToStructure<SecBuffer>(pBuffers);
				secBuffer.Dispose();
				Marshal.FreeHGlobal(pBuffers);
				pBuffers = IntPtr.Zero;
			}
		}

		public byte[] GetSecBufferBytes()
		{
			if (pBuffers == IntPtr.Zero)
				throw new ObjectDisposedException(nameof(SecBufferDesc));
			var secBuffer = Marshal.PtrToStructure<SecBuffer>(pBuffers);
			return secBuffer.GetBytes();
		}
	}

	#endregion

	#region Constants used in native Win API calls

	const int TOKEN_QUERY = 0x00008;

	const int SEC_E_OK = 0;
	const int SEC_I_CONTINUE_NEEDED = 0x90312;

	const int SECPKG_CRED_INBOUND = 1;
	const int SECPKG_CRED_OUTBOUND = 2;
	const int SECURITY_NATIVE_DREP = 0x10;

	const int MAX_TOKEN_SIZE = 12288;

	const int ISC_REQ_DELEGATE = 0x00000001;
	const int ISC_REQ_MUTUAL_AUTH = 0x00000002;
	const int ISC_REQ_REPLAY_DETECT = 0x00000004;
	const int ISC_REQ_SEQUENCE_DETECT = 0x00000008;
	const int ISC_REQ_CONFIDENTIALITY = 0x00000010;
	const int ISC_REQ_USE_SESSION_KEY = 0x00000020;
	const int ISC_REQ_PROMPT_FOR_CREDS = 0x00000040;
	const int ISC_REQ_USE_SUPPLIED_CREDS = 0x00000080;
	const int ISC_REQ_ALLOCATE_MEMORY = 0x00000100;
	const int ISC_REQ_USE_DCE_STYLE = 0x00000200;
	const int ISC_REQ_DATAGRAM = 0x00000400;
	const int ISC_REQ_CONNECTION = 0x00000800;
	const int ISC_REQ_CALL_LEVEL = 0x00001000;
	const int ISC_REQ_FRAGMENT_SUPPLIED = 0x00002000;
	const int ISC_REQ_EXTENDED_ERROR = 0x00004000;
	const int ISC_REQ_STREAM = 0x00008000;
	const int ISC_REQ_INTEGRITY = 0x00010000;
	const int ISC_REQ_IDENTIFY = 0x00020000;
	const int ISC_REQ_NULL_SESSION = 0x00040000;
	const int ISC_REQ_MANUAL_CRED_VALIDATION = 0x00080000;
	const int ISC_REQ_RESERVED1 = 0x00100000;
	const int ISC_REQ_FRAGMENT_TO_FIT = 0x00200000;

	const int SECPKG_ATTR_SIZES = 0;

	const int STANDARD_CONTEXT_ATTRIBUTES = ISC_REQ_CONFIDENTIALITY | ISC_REQ_REPLAY_DETECT | ISC_REQ_SEQUENCE_DETECT | ISC_REQ_CONNECTION;

	#endregion

	#region Prototypes of native Win API functions

	[DllImport("secur32")]
	static extern int AcquireCredentialsHandle(
		string pszPrincipal, //SEC_CHAR*
		string pszPackage, //SEC_CHAR* //"Kerberos","NTLM","Negotiative"
		int fCredentialUse,
		IntPtr PAuthenticationID,//_LUID AuthenticationID,//pvLogonID, //PLUID
		IntPtr pAuthData,//PVOID
		int pGetKeyFn, //SEC_GET_KEY_FN
		IntPtr pvGetKeyArgument, //PVOID
		out SecHandle phCredential, //SecHandle //PCtxtHandle ref
		out SecInteger ptsExpiry //PTimeStamp //TimeStamp ref
	);

	[DllImport("secur32", SetLastError = true)]
	static extern int InitializeSecurityContext(
		ref SecHandle phCredential,//PCredHandle
		IntPtr phContext, //PCtxtHandle
		string pszTargetName,
		int fContextReq,
		int Reserved1,
		int TargetDataRep,
		IntPtr pInput, //PSecBufferDesc SecBufferDesc
		int Reserved2,
		out SecHandle phNewContext, //PCtxtHandle
		ref SecBufferDesc pOutput, //PSecBufferDesc SecBufferDesc
		out uint pfContextAttr, //managed ulong == 64 bits!!!
		out SecInteger ptsExpiry //PTimeStamp
	);

	// 2 signatures of this API function needed because different usage

	[DllImport("secur32", SetLastError = true)]
	static extern int InitializeSecurityContext(
		ref SecHandle phCredential,//PCredHandle
		ref SecHandle phContext, //PCtxtHandle
		string pszTargetName,
		int fContextReq,
		int Reserved1,
		int TargetDataRep,
		ref SecBufferDesc SecBufferDesc, //PSecBufferDesc SecBufferDesc
		int Reserved2,
		out SecHandle phNewContext, //PCtxtHandle
		ref SecBufferDesc pOutput, //PSecBufferDesc SecBufferDesc
		out uint pfContextAttr, //managed ulong == 64 bits!!!
		out SecInteger ptsExpiry //PTimeStamp
	);

	[DllImport("secur32")]
	static extern int FreeCredentialsHandle(ref SecHandle phCredential); //PCredHandle

	[DllImport("secur32")]
	static extern int DeleteSecurityContext(ref SecHandle phContext); //PCtxtHandle

	#endregion

	#region Private members

	private SecHandle _clientCredentials;
	private SecHandle _clientContext;
	private bool _disposed;

	private string _securityPackage;
	private string _remotePrincipal;

	#endregion

	#region Constructors

	/// <summary>
	/// Creates SSPIHelper with default "NTLM" security package and no remote principal and gets client credentials
	/// </summary>
	public SspiHelper()
		: this("NTLM")
	{ }

	/// <summary>
	/// Creates SSPIHelper with given security package and no remote principal and gets client credentials
	/// </summary>
	/// <param name="securityPackage">Name of security package (e.g. NTLM, Kerberos, ...)</param>
	public SspiHelper(string securityPackage)
		: this(securityPackage, null)
	{ }

	/// <summary>
	/// Creates SSPIHelper with given security package and remote principal and gets client credentials
	/// </summary>
	/// <param name="securityPackage">Name of security package (e.g. NTLM, Kerberos, ...)</param>
	/// <param name="remotePrincipal">SPN of server (may be necessary for Kerberos</param>
	public SspiHelper(string securityPackage, string remotePrincipal)
	{
		_securityPackage = securityPackage;
		_remotePrincipal = remotePrincipal;
	}

	#endregion

	#region Methods

	/// <summary>
	/// Creates client security context and returns "client token"
	/// </summary>
	/// <returns>Client authentication data to be sent to server</returns>
	public byte[] InitializeClientSecurity()
	{
		EnsureDisposed();
		CloseClientContext();
		InitializeClientCredentials();
		_clientContext = new SecHandle();
		var clientTokenBuf = new SecBufferDesc(MAX_TOKEN_SIZE);
		try
		{
			var resCode = InitializeSecurityContext(
				ref _clientCredentials,
				IntPtr.Zero,
				_remotePrincipal,
				STANDARD_CONTEXT_ATTRIBUTES,
				0,
				SECURITY_NATIVE_DREP,
				IntPtr.Zero,
				0,
				out _clientContext,
				ref clientTokenBuf,
				out var contextAttributes,
				out var expiry);
			if (resCode != SEC_E_OK && resCode != SEC_I_CONTINUE_NEEDED)
				throw new Exception($"{nameof(InitializeSecurityContext)} failed");
			return clientTokenBuf.GetSecBufferBytes();
		}
		finally
		{
			clientTokenBuf.Dispose();
		}
	}

	/// <summary>
	/// Creates client authentication data based on already existing security context and
	/// authentication data sent by server
	/// This method must not be called before InitializeClientSecurity
	/// </summary>
	/// <param name="serverToken">Authentication data received from server</param>
	/// <returns>Client authentication data to be sent to server</returns>
	public byte[] GetClientSecurity(byte[] serverToken)
	{
		EnsureDisposed();
		if (_clientContext.IsInvalid)
			throw new InvalidOperationException($"{nameof(InitializeClientSecurity)} not called");
		var clientTokenBuf = new SecBufferDesc(MAX_TOKEN_SIZE);
		try
		{
			var serverTokenBuf = new SecBufferDesc(serverToken);
			try
			{
				var resCode = InitializeSecurityContext(
					ref _clientCredentials,
					ref _clientContext,
					_remotePrincipal,
					STANDARD_CONTEXT_ATTRIBUTES,
					0,
					SECURITY_NATIVE_DREP,
					ref serverTokenBuf,
					0,
					out _clientContext,
					ref clientTokenBuf,
					out var contextAttributes,
					out var expiry);
				if (resCode != SEC_E_OK && resCode != SEC_I_CONTINUE_NEEDED)
					throw new Exception($"{nameof(InitializeSecurityContext)} failed");
				return clientTokenBuf.GetSecBufferBytes();
			}
			finally
			{
				serverTokenBuf.Dispose();
			}
		}
		finally
		{
			clientTokenBuf.Dispose();
		}
	}

	#endregion

	#region Finalizer

	~SspiHelper()
	{
		Dispose(false);
	}

	#endregion

	#region IDisposable Members

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	#endregion

	#region Private methods

	private void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			_disposed = true;
			CloseClientContext();
			CloseClientCredentials();
		}
	}

	private void InitializeClientCredentials()
	{
		_clientCredentials = new SecHandle();
		var resCode = AcquireCredentialsHandle(null, _securityPackage, SECPKG_CRED_OUTBOUND,
			IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero,
			out _clientCredentials, out var expiry);
		if (resCode != SEC_E_OK)
			throw new Exception($"{nameof(AcquireCredentialsHandle)} failed");
	}

	private void CloseClientContext()
	{
		if (!_clientContext.IsInvalid)
			DeleteSecurityContext(ref _clientContext);
	}

	private void CloseClientCredentials()
	{
		if (!_clientCredentials.IsInvalid)
			FreeCredentialsHandle(ref _clientCredentials);
	}

	private void EnsureDisposed()
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(SspiHelper));
	}

	#endregion
}
