using System;
using System.Collections.Generic;
using System.Linq;


namespace Dock_Examples.Interrogator
{
	public class FabryPerotAccelerometer : SensorBase
	{
		#region -- Constants --

		private const int _FabryPerotConfigurationVersion = 1;

		private const int _MaximumNumberOfPeaksPerSensor = 100; // 20 nm span with 50 Ghz produces approximately 50 values...x2 for margin.

		private const double _ErrorAverageLength = 100;
		private const double _ErrorFilterNumerator = 1.0 / _ErrorAverageLength;
		private const double _ErrorFilterDenominator = 1.0 - _ErrorFilterNumerator;
		private const int _RecenterCountDown = 500;

		private const double _WavelengthBandHalfWidth = 7.5; // in nm

		private const double _RecenterGainDefault = 0.1;
		private const double _RecenterThresholdHighDefault = 1.0;
		private const double _RecenterThresholdLowDefault = 0.005;

		private const int _WavelengthBandFieldOffset = 0;
		private const int _CalibrationFactorFieldOffset = 1;
		private const int _RecenterGainFieldOffset = 2;
		private const int _RecenterThresholdHighFieldOffset = 3;
		private const int _RecenterThresholdLowFieldOffset = 4;

		#endregion

		#region -- Instance Variables --

		private readonly double _wavelengthBand;

		private readonly double _calibrationFactor;
		private readonly double _inverseCalibrationFactor; // avoid repeated divisions

		private readonly double _recenterGain;
		private readonly double _recenterThresholdHigh;
		private readonly double _recenterThresholdLow;

		private readonly bool _fixedOrientation;

		private readonly int _fpSensorVersion;

		#endregion


		#region -- Constructors --

		private FabryPerotAccelerometer(Guid id, string name, string model, int dutChannelIndex, double distance, double wavelengthBand, double calibrationFactor,
			double recenterGain, double recenterThresholdHigh, double recenterThresholdLow) : base(id, name, model, dutChannelIndex, distance)
		{
			// FP Sensor Version
			_fpSensorVersion = 1;

			// Center wavelength and high/low boundaries
			_wavelengthBand = wavelengthBand;

			// Store the calibration factor and store the inverse to use in multiplication that computes the output
			// in engineering units (g). This avoids the need for repeated divisions
			_calibrationFactor = calibrationFactor;
			_inverseCalibrationFactor = 1.0 / calibrationFactor;

			// Gain
			_recenterGain = recenterGain;

			// Thresholds
			_recenterThresholdHigh = recenterThresholdHigh;
			_recenterThresholdLow = recenterThresholdLow;

			// Fabry-Perot Accelerometers need to be updated every cycle regardless of user request
			IsActive = true;
		}

		private FabryPerotAccelerometer(Guid id, string name, string model, int dutChannelIndex, double distance, double wavelengthBand, double calibrationFactor,
			bool fixedOrientation) : base(id, name, model, dutChannelIndex, distance)
		{
			// FP Sensor Version
			_fpSensorVersion = 3;

			// Center wavelength and high/low boundaries
			_wavelengthBand = wavelengthBand;

			// Store the calibration factor and store the inverse to use in multiplication that computes the output
			// in engineering units (g). This avoids the need for repeated divisions
			_calibrationFactor = calibrationFactor;
			_inverseCalibrationFactor = 1.0 / calibrationFactor;

			// Gain
			_fixedOrientation = fixedOrientation;

			// Fabry-Perot Accelerometers need to be updated every cycle regardless of user request
			IsActive = true;
		}

		#endregion


		#region -- Public Properties --

		public int FPSesnorVersion => _fpSensorVersion;
		public double WavelengthBand => _wavelengthBand;

		public double CalibrationFactor => _calibrationFactor;
		public double RecenterGain => _recenterGain;
		public double RecenterThresholdHigh => _recenterThresholdHigh;
		public double RecenterThresholdLow => _recenterThresholdLow;

		public bool FixedOrientation => _fixedOrientation;

		#endregion


		#region -- Static Methods --

		internal static FabryPerotAccelerometer Create(Guid id, string name, string model, int dutChannelIndex, double distance,
			string[] fields)
		{
			// Wavelength Band
			double wavelengthBand = double.Parse(fields[_WavelengthBandFieldOffset]);

			// Calibration Factor
			double calibrationFactor = double.Parse(fields[_CalibrationFactorFieldOffset]);

			// Recenter Gain
			double recenterGain = (fields.Length > _RecenterGainFieldOffset) ?
				double.Parse(fields[_RecenterGainFieldOffset]) :
				_RecenterGainDefault;

			// Recenter ThresholdHigh
			double recenterThresholdHigh = (fields.Length > _RecenterThresholdHighFieldOffset) ?
				double.Parse(fields[_RecenterThresholdHighFieldOffset]) :
				_RecenterThresholdHighDefault;

			// Recenter ThresholdHigh
			double recenterThresholdLow = (fields.Length > _RecenterThresholdLowFieldOffset) ?
				double.Parse(fields[_RecenterThresholdLowFieldOffset]) :
				_RecenterThresholdLowDefault;


			return new FabryPerotAccelerometer(id, name, model, dutChannelIndex, distance, wavelengthBand, calibrationFactor,
				recenterGain, recenterThresholdHigh, recenterThresholdLow);
		}

		internal static FabryPerotAccelerometer Create(Guid id, string name, string model, int dutChannelIndex, double distance,
			byte[] configurationBytes, ref int offset)
		{
			// First two bytes are the Fabry-Perot configuration version
			int fabryPerotConfigurationVersion = BitConverter.ToUInt16(configurationBytes, offset);
			offset += sizeof(UInt16);

			if (fabryPerotConfigurationVersion == 1)
			{
				// Wavelength Band
				double wavelengthBand = BitConverter.ToDouble(configurationBytes, offset);
				offset += sizeof(double);

				// Calibration Factor
				double calibrationFactor = BitConverter.ToDouble(configurationBytes, offset);
				offset += sizeof(double);

				// Recenter Gain
				double recenterGain = BitConverter.ToDouble(configurationBytes, offset);
				offset += sizeof(double);

				// Recenter ThresholdHigh
				double recenterThresholdHigh = BitConverter.ToDouble(configurationBytes, offset);
				offset += sizeof(double);

				// Recenter ThresholdHigh
				double recenterThresholdLow = BitConverter.ToDouble(configurationBytes, offset);
				offset += sizeof(double);

				return new FabryPerotAccelerometer(id, name, model, dutChannelIndex, distance, wavelengthBand, calibrationFactor,
					recenterGain, recenterThresholdHigh, recenterThresholdLow);
			}
			else if (fabryPerotConfigurationVersion == 3)
			{
				// Wavelength Band
				double wavelengthBand = BitConverter.ToDouble(configurationBytes, offset);
				offset += sizeof(double);

				// Calibration Factor
				double calibrationFactor = BitConverter.ToDouble(configurationBytes, offset);
				offset += sizeof(double);

				// Fixed Orientation
				bool fixedOrientation = configurationBytes[offset] != 0;
				offset += sizeof(byte);

				return new FabryPerotAccelerometer(id, name, model, dutChannelIndex, distance, wavelengthBand, calibrationFactor,
					fixedOrientation);
			}
			else
			{
				throw new Exception($"Configuration for Sensor '{name}' contains an unrecoginized version - {fabryPerotConfigurationVersion}");
			}
		}

		#endregion

		#region -- Protected Methods --

		protected override IEnumerable<byte> GetConfigurationBytes()
		{
			return base.GetConfigurationBytes()
				.Concat(BitConverter.GetBytes((UInt16)_FabryPerotConfigurationVersion))
				.Concat(BitConverter.GetBytes(_wavelengthBand))
				.Concat(BitConverter.GetBytes(_calibrationFactor))
				.Concat(BitConverter.GetBytes(_recenterGain))
				.Concat(BitConverter.GetBytes(_recenterThresholdHigh))
				.Concat(BitConverter.GetBytes(_recenterThresholdLow));
		}

		protected override IEnumerable<string> GetConfigurationString()
		{
			return base.GetConfigurationString()
				.Concat(new string[]
				{
					"Wavelength Band (nm): " + _wavelengthBand,
					"Calibration Factor (nm/g): " + _calibrationFactor,
					"Recenter Gain: " + _recenterGain,
					"Recenter Threshold High: " + _recenterThresholdHigh,
					"Recenter Threshold Low: " + _recenterThresholdLow
				});
		}

		#endregion
	}
}
