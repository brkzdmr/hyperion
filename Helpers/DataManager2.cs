//using MicronOptics.Hyperion.Communication;
using Dock_Examples.Interrogator;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
namespace Dock_Examples.Helpers
{
    public class DataCollector
    {
        private readonly ConcurrentQueue<byte[]> dataBuffer = new ConcurrentQueue<byte[]>();

        public event EventHandler ChartUpdated;
        public event EventHandler DataUpdated;
        public int dataReceivedCount = 0;
        private Stopwatch stopwatch = new Stopwatch();
        private double dataRatehz = 0.0;
        private string Mesag;
        public static List<FBG.SpectrumChannelData> allChannelData = new List<FBG.SpectrumChannelData>();

        private readonly ConnectionManager connectionManager;
        private readonly StreamingDataReader peakDataReader;
        private readonly double offset = 2.5;
        private readonly List<string> channelNames = new List<string> { "A", "B", "C", "D" };

        public DataCollector(ConnectionManager connectionManager)
        {
            this.connectionManager = connectionManager;
            peakDataReader = new StreamingDataReader(StreamingDataMode.Peaks);
        }
        public double DataRateHz
        {
            get { return dataRatehz; }
        }
        public string Messag
        {
            get { return Mesag; }
        }
        protected virtual async Task OnChartUpdatedAsync(EventArgs e)
        {
            ChartUpdated?.Invoke(this, e);
            await Task.CompletedTask;
        }
        private void CalculateDataRate()
        {
            dataReceivedCount++;

            if (!stopwatch.IsRunning)
            {
                stopwatch.Start();
            }
            else
            {
                if (stopwatch.ElapsedMilliseconds >= 1000)
                {
                    dataRatehz = dataReceivedCount / (stopwatch.ElapsedMilliseconds / 1000.0);
                    dataReceivedCount = 0;
                    stopwatch.Restart();
                }
            }
        }
        private async Task UpdateChartAsync()
        {
            await OnChartUpdatedAsync(EventArgs.Empty);
        }

        protected virtual async Task OnDataUpdatedAsync(EventArgs e)
        {
            DataUpdated?.Invoke(this, e);
            await Task.CompletedTask;
        }

        private async Task UpdateDataAsync()
        {
            await OnDataUpdatedAsync(EventArgs.Empty);
        }

        private async Task LogAsync()
        {
            if (Sensors.Sensors.SensorList.Count > 0)
            {
                Sensors.Sensor sensor = Sensors.Sensors.SensorList[0];
                string logMessage = $"{sensor.Time.ToShortTimeString()} Değer: {sensor.SensorCurrentValue}";
                await LogToFileAsync(logMessage, "peak_data.txt");
            }
        }

        private async Task LogToFileAsync(string message, string logFilePath)
        {
            using (StreamWriter writer = File.AppendText(logFilePath))
            {
                await writer.WriteLineAsync($"{DateTime.Now} - {message}");
            }
        }

        private async Task ProcessPeakDataAsync(PeakData data)
        {
            await Task.Run(() => RunPeakScanAsync(data));
            await Task.Run(() => UpdateDataAsync());
            await Task.Run(() => LogAsync());
        }
        public void ProcessData(byte[] data)
        {
            PeakData peakData = new PeakData(data);
            Task.Run(() => ProcessPeakDataAsync(peakData));

        }
        private async Task ProcessDataFromBuffer()
        {
            while (dataBuffer.TryDequeue(out byte[] data))
            {
                ProcessData(data);
            }
        }

        private async Task GetPeakDataAsync()
        {
            CommandResponse response = await Command.ExecuteAsync(connectionManager.GetNetworkStream(), CommandName.GetPeaks);
            dataBuffer.Enqueue(response.Content);
            await ProcessDataFromBuffer();
            CalculateDataRate();
        }
        private async Task<SpectrumData> GetSpectrumDataAsync()
        {
            CommandResponse response = await Command.ExecuteAsync(connectionManager.GetNetworkStream(), CommandOptions.None, CommandName.GetSpectrum);
            SpectrumData spectrumData = response.AsSpectrumData();
            return spectrumData;
        }
        public async Task StartDataCollectionAsync()
        {
            while (true)
            {
                await GetPeakDataAsync();

            }
        }

        private async Task RunSpectrumScanAsync(SpectrumData spectrumData)
        {
            FBG.SpectrumDataVariables.wavelenghtstart = spectrumData.WavelengthStart;
            FBG.SpectrumDataVariables.wavelenghtstep = spectrumData.WavelengthStep;
            FBG.SpectrumDataVariables.wavelenghtstepcount = spectrumData.WavelengthStepCount;
            FBG.SpectrumDataVariables.channelcount = spectrumData.ChannelCount;

            for (int i = 1; i < 5; i++)
            {
                double[] spectrumValues = Array.ConvertAll(spectrumData.ToArray(i), item => (double)item);

                FBG.SpectrumChannelData channelData = new FBG.SpectrumChannelData(i, spectrumValues);
                int existingChannelIndex = allChannelData.FindIndex(data => data.ChannelId == i);
                if (existingChannelIndex != -1)
                {
                    allChannelData[existingChannelIndex] = channelData;
                }
                else
                {
                    allChannelData.Add(channelData);
                }
            }
        }
       
        private void ProcessPeakDataForChannel(double[] peakArray, int channelId, ConcurrentBag<Sensors.Sensor> updatedSensors, ConcurrentBag<Sensors.Sensor> newSensors)
        {
            foreach (double wavelength in peakArray)
            {
                Sensors.Sensor sensorToUpdate = Sensors.Sensors.SensorList.FirstOrDefault(sensor =>
                    sensor.SensorMinValue <= wavelength && wavelength <= sensor.SensorMaxValue);

                if (sensorToUpdate != null)
                {
                    sensorToUpdate.SensorCurrentValue = wavelength;
                    sensorToUpdate.SensorType = channelId.ToString();
                    updatedSensors.Add(sensorToUpdate);
                }
                else
                {
                    double minRange = wavelength - offset;
                    double maxRange = wavelength + offset;

                    string newSensorName = "Sensor-" + channelNames[channelId - 1] + wavelength;
                    string newSensorType = channelId.ToString();

                    newSensors.Add(
                        new Sensors.Sensor
                        {
                            SensorID = channelId,
                            SensorCurrentValue = wavelength,
                            SensorFirstValue = wavelength,
                            SensorMaxValue = maxRange,
                            SensorMinValue = minRange,
                            SensorName = newSensorName,
                            SensorType = newSensorType,
                            SensorTypeID = 5
                        });
                }
            }
        }

        private async Task RunPeakScanAsync(PeakData peakData)
        {
            List<Task> sensorTasks = new List<Task>();
            ConcurrentBag<Sensors.Sensor> updatedSensors = new ConcurrentBag<Sensors.Sensor>();
            ConcurrentBag<Sensors.Sensor> newSensors = new ConcurrentBag<Sensors.Sensor>();

            await Task.Run(() =>
            {
                Parallel.For(1, 5, i =>
                {
                    double[] peakArray = peakData.ToArray(i);
                    ProcessPeakDataForChannel(peakArray, i, updatedSensors, newSensors);
                });
            });

            Parallel.ForEach(updatedSensors, sensor =>
            {
                int sensorIndex = Sensors.Sensors.SensorList.IndexOf(sensor);

                if (sensorIndex != -1)
                {
                    Sensors.Sensors.SensorList[sensorIndex] = sensor;
                }
                else
                {
                }
            });

            Sensors.Sensors.SensorList.AddRange(newSensors);
        }
    }
}