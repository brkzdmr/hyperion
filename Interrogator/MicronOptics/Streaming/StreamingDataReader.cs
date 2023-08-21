using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dock_Examples.Interrogator
{
	/// <summary>
	/// The StreamingDataReader class provides a simple way to open Hyperion data streams 
	/// (peaks, full spectrum, and sensors) and reliably retrieve consecutive datasets for 
	/// every laser acquisition cycle.
	/// </summary>
	public class StreamingDataReader
	{
		#region -- Constants --

		public const int PeakTcpPort = 51972;
		public const int SpectrumTcpPort = 51973;
        public const int SensorTcpPort = 51974;

        // ----------

        #endregion

        #region -- Instance Variables --

        private byte[] _headerBuffer = new byte[CommandResponse.HeaderSizeInBytes];
		private byte[] _contentBuffer;

		#endregion


		#region -- Constructors --

		/// <summary>
		/// Create a new StreamingDataReader for the specified mode (peak, sensor, full spectrum).
		/// </summary>
		/// <param name="streamingDataMode"></param>
		public StreamingDataReader( StreamingDataMode streamingDataMode )
		{
			// Allocate content array based on stream type
			switch( streamingDataMode )
			{
				case StreamingDataMode.Peaks:
					_contentBuffer = new byte[PeakData.MaximumBufferSizeInBytes];
                    break;

				case StreamingDataMode.Spectrum:
					_contentBuffer = new byte[SpectrumData.MaximumBufferSizeInBytes];
					break;

                case StreamingDataMode.Sensor:
                    _contentBuffer = new byte[SensorData.MaximumBufferSizeInBytes];
                    break;
            }
        }

		#endregion


		#region -- Public Methods --

		/// <summary>
		/// Read the next available dataset from the input stream. The stream must be connected 
		/// and responding. Use the AsPeakData(), AsSensorData(), and AsSpectrumData() extension 
		/// methods to easily manipulated the returned buffer as the appropriate type.
		/// </summary>
		/// <param name="inputStream">The input stream (for example a TcpClient) used to communicate with the interrogator.</param>
		/// <returns>The contents of the next dataset as an array of bytes.</returns>
		public byte[] ReadStreamingData( Stream inputStream )
		{
			// Read response header from the stream
			Command.ReadSizeFromStream(
				inputStream,
				_headerBuffer );

			// Read the content buffer
			Command.ReadSizeFromStream(
				inputStream,
				_contentBuffer,
				0,
				BitConverter.ToInt32( _headerBuffer, CommandResponse.HeaderContentLengthOffsetInBytes ) );

			return _contentBuffer;
		}

		/// <summary>
		/// Asynchronously read the next available dataset from the input stream. The stream must be connected 
		/// and responding. Use the AsPeakData(), AsSensorData(), and AsSpectrumData() extension 
		/// methods to easily manipulated the returned buffer as the appropriate type.
		/// </summary>
		/// <param name="inputStream">The input stream (for example a TcpClient) used to communicate with the interrogator.</param>
		/// <returns>The contents of the next dataset as an array of bytes.</returns>
		public async Task<byte[]> ReadStreamingDataAsync( Stream inputStream )
		{
			// Read response header from the stream
			await Command.ReadSizeFromStreamAsync(
				inputStream,
				_headerBuffer );

			// Read the content buffer
			await Command.ReadSizeFromStreamAsync(
				inputStream,
				_contentBuffer,
				0,
				BitConverter.ToInt32( _headerBuffer, CommandResponse.HeaderContentLengthOffsetInBytes ) );

			return _contentBuffer;
		}

		#endregion
	}
}
