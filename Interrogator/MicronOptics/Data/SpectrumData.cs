using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dock_Examples.Interrogator
{
	public class SpectrumData
	{
		#region -- Constants --

		internal const int MaximumBufferSizeInBytes =
			HyperionData.MaximumChannelCount *
			HyperionData.MaximumAmplitudesPerChannel *
			HyperionData.AmplitudeSizeInBytes;

		// --------

		private const int _HeaderLengthOffset = 0;
		private const int _HeaderSerialNumberOffset = 8;
		private const int _HeaderTimestampOffset = 16;
		private const int _HeaderWavelengthStartOffset = 24;
		private const int _HeaderWavelengthStepOffset = 32;
		private const int _HeaderNumberOfStepsOffset = 40;
		private const int _HeaderChannelCountOffset = 44;

		#endregion

		#region -- Instance Attributes --

		private HyperionDataBuffer _hyperionDataBuffer;

		private UInt16[][] _spectra;

		private int _headerSizeInBytes;

		#endregion


		#region -- Constructors --

		internal SpectrumData( byte[] buffer )
		{
			// Use a special structure that mimics the C language "union" function. This
			// is a great high performance way to access data in byte[] as other data
			// types w/o the need for unsafe code / pointers.
			_hyperionDataBuffer.ByteBuffer = buffer;

			// The first two bytes of the buffer contains the length of the header in 
			// bytes (as a UInt16)
			_headerSizeInBytes = _hyperionDataBuffer.UInt16Buffer[0];

			// Allocate space for #channels spectrum buffers but do not allocate
			// the actual buffers unless / until they are needed.
			_spectra = new UInt16[ChannelCount][];
		}

		#endregion


		#region -- Public Properties --

		/// <summary>
		/// Get the number of optical channels present in the dataset.
		/// </summary>
		public int ChannelCount => _hyperionDataBuffer.Int16Buffer[_HeaderChannelCountOffset / sizeof( Int16 )];

		/// <summary>
		/// Get the sequential scan ID for the dataset. The instrument increments the dataset serial number
		/// each time the swept laser completes a cycle.
		/// </summary>
		public UInt64 SerialNumber => _hyperionDataBuffer.UInt64Buffer[_HeaderSerialNumberOffset / 8];

		/// <summary>
		/// Get the internal timestamp (in UTC) captured after the laser scan/acquisition completed.
		/// </summary>
		public DateTime Timestamp => new DateTime( 1970, 1, 1 ).AddSeconds(
			_hyperionDataBuffer.UInt32Buffer[_HeaderTimestampOffset / 4] +
			_hyperionDataBuffer.UInt32Buffer[_HeaderTimestampOffset / 4 + 1] / 1e9 );

		/// <summary>
		/// Get the starting wavelength (in nm) for the optical full spectrum data.
		/// </summary>
		public double WavelengthStart => _hyperionDataBuffer.DoubleBuffer[_HeaderWavelengthStartOffset / sizeof( double )];

		/// <summary>
		/// Get the wavelength increment (in nm) for each successive point in the optical full spectrum data.
		/// </summary>
		public double WavelengthStep => _hyperionDataBuffer.DoubleBuffer[_HeaderWavelengthStepOffset / sizeof( double )];

		/// <summary>
		/// Get the number of amplitude values for the optical full spectrum data.
		/// </summary>
		public int WavelengthStepCount => _hyperionDataBuffer.Int32Buffer[_HeaderNumberOfStepsOffset / sizeof( Int32 )];

		#endregion


		#region -- Public Methods --

		/// <summary>
		/// Create a copy of the full spectrum amplitudes (in dBm) from the streaming buffer for a specified channel. 
		/// </summary>
		/// <param name="channelNumber">The interrogator optical channel number (1 to N).</param>
		/// <returns>The amplitudes as an array of doubles.</returns>
		public UInt16[] ToArray( int channelNumber )
		{
			#region -- Validation --

			if (!IsValidChannel( channelNumber ))
			{
				throw new ArgumentOutOfRangeException( "Channel Number", channelNumber, "Invalid channel number." );
			}

			#endregion

			// Use channel index to access channel data
			int channelIndex = channelNumber - 1;

			if (_spectra[channelIndex] == null)
			{
				// Compute the offset to the date for the specifed channel
				int offset =
					_headerSizeInBytes +
					( channelIndex * WavelengthStepCount * HyperionData.AmplitudeSizeInBytes );

				// Allocate space to hole the wavelengths
				_spectra[channelIndex] = new UInt16[WavelengthStepCount];

				// Copy the bytes into the double[]
				Buffer.BlockCopy(
					_hyperionDataBuffer.ByteBuffer, offset,
					_spectra[channelIndex], 0, Buffer.ByteLength( _spectra[channelIndex] ) );
			}

			return _spectra[channelIndex];
		}

		/// <summary>
		/// Enumerate over the full spectrum amplitudes from the internal buffer for a specified channel. If the data simply
		/// needs to be processed before the next data arrives, this is more effcient than
		/// creating (and then garbage collecting) a new array every cycle.
		/// </summary>
		/// <param name="channelNumber">The interrogator optical channel number (1 to N).</param>
		/// <returns>The amplitudes (in dBm) as an enumerable list of doubles.</returns>
		public IEnumerable<UInt16> AsEnumerable( int channelNumber )
		{
			#region -- Validation --

			if (!IsValidChannel( channelNumber ))
			{
				throw new ArgumentOutOfRangeException( "Channel Number", channelNumber, "Invalid channel number." );
			}

			#endregion

			// Use channel index to access channel data
			int channelIndex = channelNumber - 1;

			int offsetInUInt16 =
				( _headerSizeInBytes + ( channelIndex * WavelengthStepCount * HyperionData.AmplitudeSizeInBytes ) ) /
				sizeof( UInt16 );

			for (int amplitudeIndex = 0; amplitudeIndex < WavelengthStepCount; amplitudeIndex++)
			{
				yield return _hyperionDataBuffer.UInt16Buffer[offsetInUInt16 + amplitudeIndex];
			}
		}

		#endregion

		#region -- Private Methods --

		private bool IsValidChannel( int channelNumber )
		{
			return ( ( channelNumber > 0 ) && ( channelNumber <= HyperionData.MaximumChannelCount ) );
		}

		#endregion

	}
}
