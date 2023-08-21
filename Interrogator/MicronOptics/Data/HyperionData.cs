using System;
using System.Collections.Generic;
using System.Text;

namespace Dock_Examples.Interrogator
{
	internal static class HyperionData
	{
		// ----------

		internal const int MaximumChannelCount = 16;

		// ----------

		internal const int MaximumPeaksPerChannel = 256;
		internal const int PeakWavelengthSizeInBytes = sizeof( double );

		// ----------

		internal const int SensorValueSizeInBytes = sizeof( double );

		// ----------

		internal const int MaximumAmplitudesPerChannel = 64 * 1024; // 64k points
		internal const int AmplitudeSizeInBytes = sizeof( UInt16 );
	}
}
