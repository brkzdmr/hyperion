using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dock_Examples.Interrogator
{
	/// <summary>
	/// The CommandName class provides easy access to the instrument's available commands.
	/// </summary>
	public static class CommandName
	{
		// System
		public const string Help = "#Help";
		public const string GetCommandNames = "#GetCommandNames";
		public const string GetEnabledFeatures = "#GetEnabledFeatures";
		public const string GetFirmwareVersion = "#GetFirmwareVersion";
		public const string GetFpgaVersion = "#GetFpgaVersion";
		public const string GetProductPlatform = "#GetProductPlatform";
		public const string GetSerialNumber = "#GetSerialNumber";
		public const string IsReady = "#IsReady";
		public const string Reboot = "#Reboot";

		public const string GetInstrumentName = "#GetInstrumentName";
		public const string SetInstrumentName = "#SetInstrumentName";

		public const string GetUserData = "#GetUserData";
		public const string SetUserDate = "SetUserData";


		// Detection
		public const string GetDutChannelCount = "#GetDutChannelCount";
		public const string GetMaximumPeakCountPerDutChannel = "#GetMaximumPeakCountPerDutChannel";
		public const string GetDutPeakCountMaximum = "#GetDutPeakCountMaximum";

		public const string SetPeakOffsets = "#SetPeakOffsets";


		// Acquisition
		public const string GetUserWavelengthStart = "#GetUserWavelengthStart";
		public const string GetUserWavelengthStop = "#GetUserWavelengthStop";
		public const string GetUserWavelengthDelta = "#GetUserWavelengthDelta";
		public const string GetUserWavelengthNumberOfPoints = "#GetUserWavelengthNumberOfPoints";
		public const string GetPeaks = "#GetPeaks";
		public const string GetSpectrum = "#GetSpectrum";

		public const string EnablePeakDataStreaming = "#EnablePeakDataStreaming";
		public const string DisablePeakDataStreaming = "#DisablePeakDataStreaming";
		public const string GetPeakDataStreamingStatus = "#GetPeakDataStreamingStatus";
		public const string GetPeakDataStreamingAvailableBuffer = "#GetPeakDataStreamingAvailableBuffer";
		public const string GetPeakDataStreamingDivider = "#GetPeakDataStreamingDivider";
		public const string SetPeakDataStreamingDivider = "#SetPeakDataStreamingDivider";

		public const string EnableFullSpectrumDataStreaming = "#EnableFullSpectrumDataStreaming";
		public const string DisableFullSpectrumDataStreaming = "#DisableFullSpectrumDataStreaming";
		public const string GetFullSpectrumDataStreamingStatus = "#GetFullSpectrumDataStreamingStatus";
		public const string GetFullSpectrumDataStreamingAvailableBuffer = "#GetFullSpectrumDataStreamingAvailableBuffer";
		public const string GetFullSpectrumDataStreamingDivider = "#GetFullSpectrumDataStreamingDivider";
		public const string SetFullSpectrumDataStreamingDivider = "#SetFullSpectrumDataStreamingDivider";

		public const string StreamFullSpectrumFromStorage = "#StreamFullSpectrumFromStorage";
		public const string StreamFullSpectrumToStorage = "#StreamFullSpectrumToStorage";
		public const string GetFullSpectrumStreamToStoragePercentComplete = "#GetFullSpectrumStreamToStoragePercentComplete";
		public const string GetFullSpectrumStreamToStorageAverage = "#GetFullSpectrumStreamToStorageAverage";
		public const string SetFullSpectrumStreamToStorageAverage = "#SetFullSpectrumStreamToStorageAverage";
		public const string GetFullSpectrumStreamToStorageDivider = "#GetFullSpectrumStreamToStorageDivider";
		public const string SetFullSpectrumStreamToStorageDivider = "#SetFullSpectrumStreamToStorageDivider";


		// Sensor
		public const string GetSensorNames = "#GetSensorNames";
		public const string AddSensor = "#AddSensor";
		public const string RemoveSensor = "#RemoveSensor";
		public const string GetSensorValueByName = "#GetSensorValueByName";
		public const string ExportSensors = "#ExportSensors";
		public const string SaveSensors = "#SaveSensors";


		// Laser
		public const string GetAvailableLaserScanSpeeds = "#GetAvailableLaserScanSpeeds";

		public const string GetLaserScanSpeed = "#GetLaserScanSpeed";
		public const string SetLaserScanSpeed = "#SetLaserScanSpeed";


		// Network
		public const string GetNetworkIpMode = "#GetNetworkIpMode";
		public const string EnableDynamicIpMode = "#EnableDynamicIpMode";
		public const string EnableStaticIpMode = "#EnableStaticIpMode";

		public const string GetActiveNetworkSettings = "#GetActiveNetworkSettings";

		public const string GetStaticNetworkSettings = "#GetStaticNetworkSettings";
		public const string SetStaticNetworkSettings = "#SetStaticNetworkSettings";


		// Calibration
		public const string GetPowerCalibrationInfo = "#GetPowerCalibrationInfo";


		// Host
		public const string UpdateSystem = "#UpdateSystem";

		public const string GetInstrumentUtcDateTime = "#GetInstrumentUtcDateTime";
		public const string SetInstrumentUtcDateTime = "#SetInstrumentUtcDateTime";

		public const string GetNtpEnabled = "#GetNtpEnabled";
		public const string SetNtpEnabled = "#SetNtpEnabled";

		public const string GetNtpServer = "#GetNtpServer";
		public const string SetNtpServer = "#SetNtpServer";

		public const string GetPtpEnabled = "#GetPtpEnabled";
		public const string SetPtpEnabled = "#SetPtpEnabled";
	}
}
