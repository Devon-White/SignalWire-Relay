using Newtonsoft.Json.Linq;
using SignalWire.Relay;
using SignalWire.Relay.Calling;
using SignalWire.Relay.Messaging;
using System;
using System.Collections.Generic;
using Client = SignalWire.Relay.Client;

public static class Globals
{
    public const String SW_num = "Your SW Number Here";
    public const String To_num = "Destination Number Here";
}


namespace Example
{
    internal class ReadyConsumer : Consumer
    {

        protected override void Setup()
        {
            Project = "Project ID Here";
            Token = "Auth Token Here";
            Contexts = new List<string> { "Context Here" };
        }

        protected override void Ready()
        {
            try
            {

                Console.WriteLine("Client ready, sending message...");
                SendResult result = Client.Messaging.Send("test", Globals.To_num, Globals.From_num, new SendSource("Respond With Y to see the answer to 2+2\nRespond With N if you hate math"));

            }

            finally
            {
                Console.WriteLine("Message Sent");
            }
        }


        protected override void OnIncomingCall(Call call)
        {
            Console.WriteLine(call);
            Console.WriteLine(call.Type);
            AnswerResult resultAnswer = call.Answer();
            var From_num = resultAnswer.Event.Payload["device"]["params"]["from_number"];
            Console.WriteLine(From_num);
            call.PlayTTS("Welcome to SignalWire!");
            PlayAction actionPlay = call.PlayAudioAsync("https://cdn.signalwire.com/default-music/welcome.mp3");
            Thread.Sleep(5000);
            actionPlay.Stop();
            ConnectResult resultConnect = call.Connect(new List<List<CallDevice>>
            {
                new List<CallDevice>
                {
                    new CallDevice
                    {
                        Type = CallDevice.DeviceType.phone,
                        Parameters = new CallDevice.PhoneParams
                        {
                            ToNumber = Globals.To_num,
                            FromNumber = (string)From_num,
                            Timeout = 30,
                        }
                    }
                }});
            if (resultConnect.Successful)
            {
               Console.WriteLine("Call has been answered");
               call.PlayTTS("We have answered the call. Now ending the call");
               call.Hangup();

            }

        }
        protected override void OnIncomingMessage(Message message)
        {
           var msg_body = message.Body.ToLower();
           var cus_num = message.From;
            Console.WriteLine(message.From);
            if (msg_body == "y")
            {
                Client.Messaging.Send("test", cus_num, Globals.From_num, new SendSource("The answer to 2+2 is 4!"));

            }
            else if (msg_body == "n")
                        {
                Client.Messaging.Send("test", cus_num, Globals.From_num, new SendSource("Wow, what a math hater"));
            }
            
            else
            {
                Client.Messaging.Send("test", cus_num, Globals.From_num, new SendSource("Wrong input"));
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
