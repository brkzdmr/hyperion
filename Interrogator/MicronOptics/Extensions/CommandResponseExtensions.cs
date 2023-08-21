using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dock_Examples.Interrogator
{
	public static class CommandResponseExtensions
	{
		public static PeakData AsPeakData( this CommandResponse response ) => new PeakData( response.Content );

		public static SensorData AsSensorData( this CommandResponse response ) => new SensorData( response.Content );

		public static SpectrumData AsSpectrumData( this CommandResponse response ) => new SpectrumData( response.Content );

		// ------

		public static string AsString(this CommandResponse response) => ASCIIEncoding.ASCII.GetString(response.Content);

		public static double AsDouble( this CommandResponse response ) => BitConverter.ToDouble( response.Content, 0 );

		public static double AsFloat( this CommandResponse response ) => BitConverter.ToDouble( response.Content, 0 );

		public static double AsInt16( this CommandResponse response ) => BitConverter.ToInt16( response.Content, 0 );

		public static double AsUInt16( this CommandResponse response ) => BitConverter.ToUInt16( response.Content, 0 );

		public static double AsInt32( this CommandResponse response ) => BitConverter.ToInt32( response.Content, 0 );

		public static double AsUInt32( this CommandResponse response ) => BitConverter.ToUInt32( response.Content, 0 );

		public static double AsInt64( this CommandResponse response ) => BitConverter.ToInt64( response.Content, 0 );

		public static double AsUInt64( this CommandResponse response ) => BitConverter.ToUInt64( response.Content, 0 );
	}
}
