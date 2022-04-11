using SignalWire.Relay;
using SignalWire.Relay.Calling;
namespace Example
{
    internal class ReadyConsumer : Consumer
    {
        protected override void Setup()
        {
            Project = "PROJECT HERE";
            Token = "TOKEN HERE";
            Contexts = new List<string> { "test" };
            // Do additional setup here
        }

        protected override void OnIncomingCall(Call call)
        {
            AnswerResult resultAnswer = call.Answer();
            Console.WriteLine("Call has been answered");
            call.PlayTTS("Welcome to SignalWire!");
            Console.WriteLine("We are saying welcome to signalwire");
            PlayAction actionPlay = call.PlayAudioAsync("https://cdn.signalwire.com/default-music/welcome.mp3");
            Console.WriteLine("Playing SignalWire Intro Music for 5 seconds");
            Thread.Sleep(5000);
            StopResult resultStop = actionPlay.Stop();
            ConnectResult resultConnect = call.Connect(new List<List<CallDevice>>

            {
                new List<CallDevice>
                {
                    new CallDevice
                    {
                        Type = CallDevice.DeviceType.phone,
                        Parameters = new CallDevice.PhoneParams
                        {
                            ToNumber = "+1XXXXXXXXXX",
                            FromNumber = "+1XXXXXXXXXX",
                            Timeout = 30,
                        }
                    }
                }});
                if (resultConnect.Successful) {
                Console.WriteLine("Call has been answered");
            
                // The call was connected, and is available at resultConnect.Call
            }
        }
    }

    internal class Program
    {
        public static void Main()
        {
            new ReadyConsumer().Run();
        }
    }
}
