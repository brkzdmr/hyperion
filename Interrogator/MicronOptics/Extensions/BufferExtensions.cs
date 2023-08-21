using System;
using System.Collections.Generic;
using System.Text;

namespace Dock_Examples.Interrogator
{
    public static class BufferExtensions
    {
		public static PeakData AsPeakData( this byte[] buffer ) => new PeakData( buffer );

		public static SensorData AsSensorData( this byte[] buffer ) => new SensorData( buffer );

		public static SpectrumData AsSpectrumData( this byte[] buffer ) => new SpectrumData( buffer );
	}
}
