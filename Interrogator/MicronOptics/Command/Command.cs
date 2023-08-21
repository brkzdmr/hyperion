using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dock_Examples.Interrogator
{
	public static partial class Command
	{
		#region -- Constants --

		public const int TcpPort = 51971;

		#endregion


		#region -- Public Methods --

		#region -- Async --

		public static async Task<CommandResponse> ExecuteAsync(Stream stream, string name, params object[] arguments)
		{
			// Send
			return await ExecuteAsync(
				stream,
				CommandOptions.None,
				name,
				ASCIIEncoding.ASCII.GetBytes(string.Join(" ", arguments)));
		}

		public static async Task<CommandResponse> ExecuteAsync(Stream stream, CommandOptions options, string name, params object[] arguments)
		{
			// Send
			return await ExecuteAsync(
				stream,
				options,
				name,
				ASCIIEncoding.ASCII.GetBytes(string.Join(" ", arguments)));
		}

		public static async Task<CommandResponse> ExecuteAsync(Stream stream, CommandOptions options, string name, byte[] content = null)
		{
			await WriteToStreamAsync(stream, options, name, content);
			return await ReadCommandResponseAsync(stream);
		}

		#endregion

		#region -- Blocking --

		public static CommandResponse Execute(Stream stream, string name, params object[] arguments)
		{
			// Send
			return Execute(
				stream,
				CommandOptions.None,
				name,
				ASCIIEncoding.ASCII.GetBytes(string.Join(" ", arguments)));
		}

		public static CommandResponse Execute(Stream stream, CommandOptions options, string name, params object[] arguments)
		{
			// Send
			return Execute(
				stream,
				options,
				name,
				ASCIIEncoding.ASCII.GetBytes(string.Join(" ", arguments)));
		}

		public static CommandResponse Execute(Stream stream, CommandOptions options, string name, byte[] content = null)
		{
			WriteToStream(stream, options, name, content);
			return ReadCommandResponse(stream);
		}

		#endregion

		#endregion

		#region -- Private Methods --

		#region -- Async --

		// -- Write --

		internal static async Task WriteToStreamAsync(Stream outputStream, CommandOptions options, string name, byte[] content = null)
		{
			// Protect against null content (no arguments)
			content = content ?? new byte[0];

			// Compute and write the 8 byte header header
			UInt64 header =
				((UInt64)options) +
				((UInt64)name.Length << 16) +
				((UInt64)content.Length << 32);

			await outputStream.WriteAsync(BitConverter.GetBytes(header), 0, sizeof(UInt64));

			// Write name to stream
			await outputStream.WriteAsync(ASCIIEncoding.ASCII.GetBytes(name), 0, name.Length);

			// Write content to stream
			await outputStream.WriteAsync(content, 0, content.Length);

			// For buffered streams (i am looking at you windows 10) a flush is required in order to send
			// the data. For unbuffered streams like .Net NetworkStream, its a no op.
			await outputStream.FlushAsync();
		}


		// -- Read --

		internal static async Task<CommandResponse> ReadCommandResponseAsync(Stream inputStream, bool createNewContentBuffer = false)
		{
			// Read response header from the stream
			byte[] responseHeaderBuffer = new byte[CommandResponse.HeaderSizeInBytes];

			await ReadSizeFromStreamAsync(inputStream, responseHeaderBuffer);

			// Read the message buffer
			byte[] messageBuffer = new byte[BitConverter.ToUInt16(responseHeaderBuffer, CommandResponse.HeaderMessageLengthOffsetInBytes)];

			if (messageBuffer.Length > 0)
			{
				await ReadSizeFromStreamAsync(inputStream, messageBuffer);
			}

			// Read the content buffer
			byte[] contentBuffer = new byte[BitConverter.ToUInt32(responseHeaderBuffer, CommandResponse.HeaderContentLengthOffsetInBytes)];

			if (contentBuffer.Length > 0)
			{
				await ReadSizeFromStreamAsync(inputStream, contentBuffer);
			}

			// Create and return corresponding command response
			return new CommandResponse(
				(CommandStatus)responseHeaderBuffer[CommandResponse.HeaderStatusOffsetInBytes],
				(CommandOptions)responseHeaderBuffer[CommandResponse.HeaderOptionOffsetInBytes],
				ASCIIEncoding.ASCII.GetString(messageBuffer, 0, messageBuffer.Length),
				contentBuffer);
		}

		internal static async Task ReadSizeFromStreamAsync(Stream inputStream, byte[] buffer, int offset = 0, int? size = null)
		{
			int bytesRead = 0;

			int count = size ?? buffer.Length;
			int zeroByteCount = 0;

			// Attempt to read the requested number of bytes. It may require multiple reads from the commandStream.
			while (bytesRead < count)
			{
				bytesRead += await inputStream.ReadAsync(buffer, offset + bytesRead, count - bytesRead);

				// Sometimes things go wrong in the middle of multiple reads...eventually give up 
				// and throw an exception.
				if ((bytesRead == 0) && (++zeroByteCount == 10))
				{
					throw new Exception(string.Format("Error reading instrument responseBuffer. {0} of {1} bytes read.", bytesRead, count));
				}
			}
		}

		#endregion

		#region -- Blocking --

		// -- Write --

		internal static void WriteToStream(Stream outputStream, CommandOptions options, string name, params object[] arguments)
		{
			// Combine arguments with spaces
			WriteToStream(
				outputStream,
				options,
				name,
				ASCIIEncoding.ASCII.GetBytes(string.Join(" ", arguments)));
		}

		internal static void WriteToStream(Stream outputStream, CommandOptions options, string name, byte[] content = null)
		{
			// Protect against null content (no arguments)
			content = content ?? new byte[0];

			// Compute and write the 8 byte header header
			UInt64 header =
				((UInt64)options) +
				((UInt64)name.Length << 16) +
				((UInt64)content.Length << 32);

			outputStream.Write(BitConverter.GetBytes(header), 0, sizeof(UInt64));

			// Write name to stream
			outputStream.Write(ASCIIEncoding.ASCII.GetBytes(name), 0, name.Length);

			// Write content to stream
			outputStream.Write(content, 0, content.Length);

			// For buffered streams (i am looking at you windows 10) a flush is required in order to send
			// the data. For unbuffered streams like .Net NetworkStream, its a no op.
			outputStream.Flush();
		}


		// -- Read --

		internal static CommandResponse ReadCommandResponse(Stream inputStream, bool createNewContentBuffer = false)
		{
			// Read response header from the stream
			byte[] responseHeaderBuffer = new byte[CommandResponse.HeaderSizeInBytes];

			ReadSizeFromStream(inputStream, responseHeaderBuffer);

			// Read the message buffer
			byte[] messageBuffer = new byte[BitConverter.ToUInt16(responseHeaderBuffer, CommandResponse.HeaderMessageLengthOffsetInBytes)];

			if (messageBuffer.Length > 0)
			{
				ReadSizeFromStream(inputStream, messageBuffer);
			}

			// Read the content buffer
			byte[] contentBuffer = new byte[BitConverter.ToUInt32(responseHeaderBuffer, CommandResponse.HeaderContentLengthOffsetInBytes)];

			if (contentBuffer.Length > 0)
			{
				ReadSizeFromStream(inputStream, contentBuffer);
			}

			// Create and return corresponding command response
			return new CommandResponse(
				(CommandStatus)responseHeaderBuffer[CommandResponse.HeaderStatusOffsetInBytes],
				(CommandOptions)responseHeaderBuffer[CommandResponse.HeaderOptionOffsetInBytes],
				ASCIIEncoding.ASCII.GetString(messageBuffer, 0, messageBuffer.Length),
				contentBuffer);
		}

		internal static void ReadSizeFromStream(Stream inputStream, byte[] buffer, int offset = 0, int? size = null)
		{
			int bytesRead = 0;

			int count = size ?? buffer.Length;
			int zeroByteCount = 0;

			// Attempt to read the requested number of bytes. It may require multiple reads from the commandStream.
			while (bytesRead < count)
			{
				bytesRead += inputStream.Read(buffer, offset + bytesRead, count - bytesRead);

				// Sometimes things go wrong in the middle of multiple reads...eventually give up 
				// and throw an exception.
				if ((bytesRead == 0) && (++zeroByteCount == 10))
				{
					throw new Exception(string.Format("Error reading instrument responseBuffer. {0} of {1} bytes read.", bytesRead, count));
				}
			}
		}

		#endregion

		#endregion
	}
}
