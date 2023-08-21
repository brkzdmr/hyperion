using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dock_Examples.Interrogator
{
	public class PeakData
	{
		#region -- Constants --

		internal const int MaximumBufferSizeInBytes =
			HyperionData.MaximumChannelCount *
			HyperionData.MaximumPeaksPerChannel *
			HyperionData.PeakWavelengthSizeInBytes;

		// -------- 

		private const int _HeaderLengthOffset = 0;
		private const int _HeaderStatusOffset = 2;
		private const int _HeaderAvailabeBuffferPercentageOffset = 3;
		private const int _HeaderSerialNumberOffset = 8;
		private const int _HeaderTimestampOffset = 16;
		private const int _HeaderFirstPerChannelPeakCountOffset = 24;

		#endregion

		#region -- Instance Attributes --

		private HyperionDataBuffer _hyperionDataBuffer;

		private Int16[] _channelPeakWavelengthCounts = new Int16[HyperionData.MaximumChannelCount];
		private int[] _channelPeakWavelengthOffsets = new int[HyperionData.MaximumChannelCount];

		private double[][] _peakWavelengths = new double[HyperionData.MaximumChannelCount][];

		#endregion


		#region -- Constructors --

		internal PeakData( byte[] buffer )
		{
			// Use a special structure that mimics the C language "union" function. This
			// is a great high performance way to access data in byte[] as other data
			// types w/o the need for unsafe code / pointers.
			_hyperionDataBuffer.ByteBuffer = buffer;

			// Copy the peak wavelength count per channel into an array that makes accessing them easier.
			Buffer.BlockCopy(
				buffer, _HeaderFirstPerChannelPeakCountOffset,
				_channelPeakWavelengthCounts, 0, Buffer.ByteLength( _channelPeakWavelengthCounts ) );

			// Precompute the offset to the data for each channel. The first two bytes of the 
			// buffer are the header length.
			_channelPeakWavelengthOffsets[0] = _hyperionDataBuffer.UInt16Buffer[0];

			for (int channelIndex = 1; channelIndex < HyperionData.MaximumChannelCount; channelIndex++)
			{
				_channelPeakWavelengthOffsets[channelIndex] =
					_channelPeakWavelengthOffsets[channelIndex - 1] +
					( _channelPeakWavelengthCounts[channelIndex - 1] * HyperionData.PeakWavelengthSizeInBytes );
			}
		}

		#endregion


		#region -- Public Properties --

		/// <summary>
		/// Get the per channel number of peak wavelengths.
		/// </summary>
		public short[] ChannelPeakWavelengthCounts => _channelPeakWavelengthCounts;

		/// <summary>
		/// Get the instrument data acquisition status.
		/// </summary>
		//public byte Status => _hyperionDataBuffer.ByteBuffer[_PeakHeaderStatusOffset];

		/// <summary>
		/// Get the percentage of the internal streaming buffer that is current availble. A percentage near
		/// of 100 means that the connection is capable of reading the data at the rate it is being produced. If
		/// the client is unable to read the data at the rate it is produced the instrument will begin to fill
		/// its internal buffer to prevent data loss. If the internal buffer fills it will reset and the data
		/// from that time period will be lost.
		/// </summary>
		public byte AvailabeBufferPercentage => _hyperionDataBuffer.ByteBuffer[_HeaderAvailabeBuffferPercentageOffset];

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

		#endregion


		#region -- Public Methods --

		/// <summary>
		/// Create a copy of the peak wavelength values from the streaming buffer for a specified channel. 
		/// </summary>
		/// <param name="channelNumber">The interrogator optical channel number (1 to N).</param>
		/// <returns>The peak wavelengths as an array of doubles.</returns>
		public double[] ToArray( int channelNumber )
		{
			#region -- Validation --

			if (!IsValidChannel( channelNumber ))
			{
				throw new ArgumentOutOfRangeException( "Channel Number", channelNumber, "Invalid channel number." );
			}

			#endregion

			// Use channel index to access channel data
			int channelIndex = channelNumber - 1;

			if (_peakWavelengths[channelIndex] == null)
			{
				// Allocate space to hole the wavelengths
				_peakWavelengths[channelIndex] = new double[_channelPeakWavelengthCounts[channelIndex]];

				// Copy the bytes into the double[]
				Buffer.BlockCopy(
					_hyperionDataBuffer.ByteBuffer, _channelPeakWavelengthOffsets[channelIndex],
					_peakWavelengths[channelIndex], 0, Buffer.ByteLength( _peakWavelengths[channelIndex] ) );
			}

			return _peakWavelengths[channelIndex];
		}

		/// <summary>
		/// Enumerate over the peak wavelengths from the internal buffer for a specified channel. If the data simply
		/// needs to be processed before the next data arrives, this is more effcient than
		/// creating (and then garbage collecting) a new array every cycle.
		/// </summary>
		/// <param name="channelNumber">The interrogator optical channel number (1 to N).</param>
		/// <returns>The peak wavelengths (in nm) as an enumerable list of doubles.</returns>
		public IEnumerable<double> AsEnumerable( int channelNumber )
		{
			#region -- Validation --

			if (!IsValidChannel( channelNumber ))
			{
				throw new ArgumentOutOfRangeException( "Channel Number", channelNumber, "Invalid channel number." );
			}

			#endregion

			// Use channel index to access channel data
			int channelIndex = channelNumber - 1;

			int offsetInDoubles = _channelPeakWavelengthOffsets[channelIndex] / sizeof( double );

			for (int peakIndex = 0; peakIndex < _channelPeakWavelengthCounts[channelIndex]; peakIndex++)
			{
				yield return _hyperionDataBuffer.DoubleBuffer[offsetInDoubles + peakIndex];
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
