using System;
using System.Collections.Generic;
using System.Text;

namespace Dock_Examples.Interrogator
{
	public class SensorData
	{
		#region -- Constants --

		internal const int MaximumBufferSizeInBytes =
			HyperionData.MaximumChannelCount *
			HyperionData.MaximumPeaksPerChannel *
			HyperionData.SensorValueSizeInBytes;

		// --------

		private const int _HeaderLengthOffset = 0;
		private const int _HeaderStatusOffset = 2;
		private const int _HeaderAvailabeBuffferPercentageOffset = 3;
		private const int _HeaderSerialNumberOffset = 8;
		private const int _HeaderTimestampOffset = 16;
		private const int _HeaderSensorCountOffset = 24;

		#endregion

		#region -- Instance Attributes --

		private HyperionDataBuffer _hyperionDataBuffer;

		private int _headerLengthInBytes;

		private int _sensorCount;
		private double[] _sensorValues;

		#endregion


		#region -- Constructors --

		internal SensorData( byte[] buffer )
		{
			// Use a special structure that mimics the C language "union" function. This
			// is a great high performance way to access data in byte[] as other data
			// types w/o the need for unsafe code / pointers.
			_hyperionDataBuffer.ByteBuffer = buffer;

			// Sensor Count
			_sensorCount = _hyperionDataBuffer.UInt16Buffer[_HeaderSensorCountOffset / sizeof( UInt16 )];

			// Header Offset In Bytes
			_headerLengthInBytes = _hyperionDataBuffer.UInt16Buffer[_HeaderLengthOffset / sizeof( UInt16 )];
		}

		#endregion


		#region -- Public Properties --

		/// <summary>
		/// Get the number of sensors in the dataset.
		/// </summary>
		public int NumberOfSensors => _sensorCount;

		/// <summary>
		/// Get the instrument data acquisition status.
		/// </summary>
		public byte Status => _hyperionDataBuffer.ByteBuffer[_HeaderStatusOffset];

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
		/// Create a copy of the streaming sensor values from the streaming buffer. 
		/// </summary>
		/// <returns>The sensor data values as an array of doubles.</returns>
		public double[] ToArray()
		{
			if (_sensorValues == null)
			{
				_sensorValues = new double[_sensorCount];

				Buffer.BlockCopy(
					_hyperionDataBuffer.ByteBuffer, _headerLengthInBytes,
					_sensorValues, 0, Buffer.ByteLength( _sensorValues ) );
			}

			return _sensorValues;
		}

		/// <summary>
		/// Enumerate over the streaming sensor values from the internal buffer. If the data simply
		/// needs to be processed before the next data arrives, this is more effcient than
		/// creating (and then garbage collecting) a new array every cycle.
		/// </summary>
		/// <returns>The sensor data values as an enumerable list of doubles.</returns>
		public IEnumerable<double> AsEnumerable()
		{
			int offsetInDoubles = _headerLengthInBytes / sizeof( double );

			for (int sensorIndex = 0; sensorIndex < _sensorCount; sensorIndex++)
			{
				yield return _hyperionDataBuffer.DoubleBuffer[offsetInDoubles + sensorIndex];
			}
		}

		public double GetValue(int index)
		{
			return _hyperionDataBuffer.DoubleBuffer[4 + index];
		}

		#endregion
	}
}
