using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace Dock_Examples.Interrogator
{
	public abstract class SensorBase
	{
		#region -- Constants --

		private const int _BaseConfigurationVersion = 1;

		// ---

		private const int _ModelFieldOffset = 0;
		private const int _DutChannelNumberFieldOfset = 1;
		private const int _DistanceFieldOffset = 2;

		private const int _NumberOfSensorBaseParsedFields = 3;

		// --

		private const int _IdSizeInBytes = 16;

		#endregion


		#region -- Constructors --

		protected SensorBase(Guid id, string name, string model, int dutChannelIndex, double distance)
		{
			Id = id;

			Name = name;
			Model = model;

			DutChannelIndex = dutChannelIndex;

			Distance = distance;
		}

		#endregion


		#region -- Internal Properties --

		public Guid Id { get; private set; }

		public string Name { get; private set; }
		public string Model { get; private set; }

		public int DutChannelIndex { get; private set; }
		public int ChannelIndex { get; private set; }

		public double Distance { get; private set; }

		public bool IsActive { get; set; }
		public bool IsAvailable { get; set; } = true;

		#endregion


		#region -- Static Methods --

		public static SensorBase Create(string name, params string[] fields)
		{
			// Uniquie ID
			Guid id = Guid.NewGuid();

			// Model
			string model = fields[SensorBase._ModelFieldOffset];

			// DUT Channel Index
			int dutChannelIndex = Convert.ToInt32(fields[SensorBase._DutChannelNumberFieldOfset]) - 1;

			// Distance
			double distance = Convert.ToInt32(fields[SensorBase._DistanceFieldOffset]);

			// Extract the remaining fields to pass to the specific sensor for parsing
			string[] sensorSpecificFields = fields
				.Skip(_NumberOfSensorBaseParsedFields)
				.ToArray();

			switch (model.ToLower())
			{
				case SensorModel.os7510:
				case SensorModel.os7520:
					return FabryPerotAccelerometer.Create(id, name, model, dutChannelIndex, distance, sensorSpecificFields);

				default:
					throw new Exception("Sensor Model/Type " + model + " is unknown");
			}
		}

		public static SensorBase Create(byte[] configurationBytes, ref int offset)
		{
			// First two bytes are the configuration version
			int baseConfigurationVersion = BitConverter.ToUInt16(configurationBytes, offset);
			offset += sizeof(UInt16);

			// Config ID = 1 did not have name. Name was added for #ExportSensors. Prior Name was used
			// as the key into the configuration and was not needed. Yes. I know. It was a dumb decision.
			if (baseConfigurationVersion == 2) 
			{
				// ID (GUID)
				Guid id = new Guid(configurationBytes.Skip(offset).Take(_IdSizeInBytes).ToArray());
				offset += _IdSizeInBytes;

				// Name
				int length = BitConverter.ToUInt16(configurationBytes, offset);
				offset += 2;

				string name = ASCIIEncoding.ASCII.GetString(configurationBytes, offset, length);
				offset += length;

				// Next two bytes are the length of the Model string (stored at UInt16 to save space)
				length = BitConverter.ToUInt16(configurationBytes, offset);
				offset += sizeof(UInt16);

				// Next bytes are the Model string
				string model = ASCIIEncoding.ASCII.GetString(configurationBytes, offset, length);
				offset += length;

				// DUT Channel Index (stored at UInt16 to save space)
				int dutChannelIndex = BitConverter.ToUInt16(configurationBytes, offset);
				offset += sizeof(UInt16);

				// Distance
				double distance = BitConverter.ToDouble(configurationBytes, offset);
				offset += sizeof(double);

				switch (model.ToLower())
				{
					case SensorModel.os7510:
					case SensorModel.os7520:
						return FabryPerotAccelerometer.Create(id, name, model, dutChannelIndex, distance,
							configurationBytes, ref offset);

					default:
						Debug.WriteLine("Sensor Model/Type " + model + " is unknown");
						throw new ArgumentException("Sensor Model/Type " + model + " is unknown");
				}
			}

			return null;
		}

		#endregion

		#region -- Internal Methods --

		internal byte[] ExportToBytes()
		{
			return GetConfigurationBytes()
				.ToArray();
		}

		internal string ExportToString()
		{
			return
				"\r\n\t" +
				string.Join("\r\n\t", GetConfigurationString());
		}

		#endregion

		#region -- Protected Methods --

		protected virtual IEnumerable<byte> GetConfigurationBytes()
		{
			return BitConverter.GetBytes((UInt16)_BaseConfigurationVersion)     // Configuration Version
				.Concat(Id.ToByteArray())                                           // ID
				.Concat(BitConverter.GetBytes((UInt16)Model.Length))            // Model - String Length
				.Concat(ASCIIEncoding.ASCII.GetBytes(Model))                    // Model
				.Concat(BitConverter.GetBytes((UInt16)DutChannelIndex))     // DUT Channel Index
				.Concat(BitConverter.GetBytes(Distance));                       // Distance
		}

		protected virtual IEnumerable<string> GetConfigurationString()
		{
			return new string[] {
				"ID: " + Id,
				"Model: " + Model,
				"Channel #: " + ( DutChannelIndex + 1 ) + ( IsAvailable ? "" : " (Channel is unavailable)" ),
				"Distance (m): " + Distance ,
				"Is Available: " + IsAvailable,
			};
		}

		#endregion
	}
}

