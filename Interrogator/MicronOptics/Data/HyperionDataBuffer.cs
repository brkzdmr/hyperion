using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Dock_Examples.Interrogator
{
	[StructLayout( LayoutKind.Explicit )]
	internal struct HyperionDataBuffer
	{
		[FieldOffset( 0 )]
		public byte[] ByteBuffer;

		[FieldOffset( 0 )]
		public Int16[] Int16Buffer;

		[FieldOffset( 0 )]
		public UInt16[] UInt16Buffer;

		[FieldOffset( 0 )]
		public Int32[] Int32Buffer;

		[FieldOffset( 0 )]
		public UInt32[] UInt32Buffer;

		[FieldOffset( 0 )]
		public UInt64[] UInt64Buffer;

		[FieldOffset( 0 )]
		public double[] DoubleBuffer;
	}
}
