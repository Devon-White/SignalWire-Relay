using System.Diagnostics;
using SignalWire.Relay;
using SignalWire.Relay.Calling;
using SignalWire.Relay.Messaging;
using Client = SignalWire.Relay.Client;

public static class Globals
{
    public const String SW_num = "SW Number HERE";
    public const String PSTN_To_num = "Personal Here";
}

namespace Example
{
    internal class ReadyConsumer : Consumer
    {
        protected override void Setup()
        {
            Project = "Project ID Here";
            Token = "Auth Token Here;
            Contexts = new List<string> { "test" };
        }

        protected override void Ready()
        {
            try
            {

                Console.WriteLine("Client ready, sending message...");
                SendResult result = Client.Messaging.Send("test", Globals.PSTN_To_num, Globals.SW_num, new SendSource("Respond With Y to receive a call\n respond with N to ignore my request."));

            }

            finally
            {
                Console.WriteLine("Message Sent");
            }
        }


        protected override void OnIncomingCall(Call call)
        {
            var cur_call = (PhoneCall)call;
            AnswerResult resultAnswer = call.Answer();
            Welcome(cur_call, Client);
        }

        private static Client GetClient(Client client)
        {
            return client;
        }


        private static async void Menu(PhoneCall call, Client client)
        {
            var resultPrompt = call.PromptTTS(
                "Please say. Alpha. To be transferred to a PSTN line, or please say. Beta. To hear a affirmation.",
                new CallCollect
                {
                    InitialTimeout = 10,
                    Speech = new CallCollect.SpeechParams
                    {
                    }
                },
                gender: "female",
                volume: 4.0);

            if (resultPrompt.Successful)
            {
                var From_num = call.To;
                if (From_num == Globals.SW_num)
                {
                    From_num = call.From;
                }

                Console.WriteLine(resultPrompt.Result);
                if (resultPrompt.Result.ToLower() == "alpha")
                {

                    ConnectResult plan = call.Connect(new List<List<CallDevice>>
                    {
                        new List<CallDevice>
                        {
                            new CallDevice
                            {
                                Type = CallDevice.DeviceType.phone,
                                Parameters = new CallDevice.PhoneParams
                                {
                                    ToNumber = Globals.PSTN_To_num,
                                    FromNumber = (string)From_num,
                                    Timeout = 30,
                                }
                            }
                        }
                    });
                    if (plan.Successful)
                    {
                        Call ogCall = plan.Call;
                        Call_connected(call, ogCall);
                    }
                    else
                    {
                        Console.WriteLine("Called Connection failed. Ending process...");
                        call.Disconnect();
                    }
                }

                else if (resultPrompt.Result.ToLower() == "beta")
                {
                    await Request(call, GetClient(client));
                }

                else
                {
                    call.PlayTTS("Sorry, we did not recognize your response.");
                    Menu(call, client);
                }
            }
            else
            {
                call.PlayTTS("Sorry, we did not recognize your response");
                Menu(call, client);
            }
        }

        private protected static void Welcome(Call call, Client client)
        {
            var z = (PhoneCall)call;
            call.PlayTTS("Welcome to SignalWire!");
            var playAction = call.PlayAudioAsync("https://cdn.signalwire.com/default-music/welcome.mp3");
            Thread.Sleep(5000);
            playAction.Stop();
            Thread.Sleep(1000);
            Menu((PhoneCall)call, GetClient(client));

        }
        protected static void Call_connected(Call call, Call peer)
        {
            RecordAction actionRecord = call.RecordAsync(
                new CallRecord
                {
                    Audio = new CallRecord.AudioParams

                    {
                        Beep = true,
                        Stereo = true,
                        Direction = CallRecord.AudioParams.AudioDirection.both,
                        EndSilenceTimeout = 20
                    }
                });

            Thread.Sleep(1000);
            if (actionRecord.Payload != null)
            {

                Console.WriteLine("Call has been answered");
                call.PlayTTS("We have answered the call.");
                peer.PlayTTS("I am peer 2. Please say. I am peer 2!");
                Thread.Sleep(5000);
                peer.PlayTTS("Hanging up call and saving recording.");
                peer.Hangup();
                call.PlayTTS("Peer has disconnected. Saving recording and ending call.");
                StopResult resultStop = actionRecord.Stop();
                Console.WriteLine(actionRecord.Url);
                call.Hangup();


            }
            else
            {
                Console.WriteLine("Recording has failed, ending call...");
                peer.Hangup();
                call.Hangup();
            }
        }


        public static async Task<object> Request(PhoneCall call, Client client)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://www.affirmations.dev");
            string res = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(res);
            char[] charsToTrim = { '{', '}', '"', ':', '"' };
            const string toRemove = "affirmation";
            var i = res.IndexOf(toRemove, StringComparison.Ordinal);
            if (i >= 0)
            {
                var almost = res.Remove(i, toRemove.Length);

                string partly = almost.Trim(charsToTrim);
                string Final = partly;
                Console.WriteLine(Final);
                call.PlayTTS($"{Final}");
                Menu(call, GetClient(client));
            }
            return res;
        }

        protected override void OnIncomingMessage(Message message)
        {
            var msg_body = message.Body.ToLower();
            var cus_num = message.From;
            Console.WriteLine(message.From);
            if (msg_body == "y")
            {
                Client.Messaging.Send("test", cus_num, Globals.SW_num, new SendSource($"Sending you a call now from " + $"{Globals.SW_num}"));
                Call call = Client.Calling.NewPhoneCall(cus_num, Globals.SW_num);
                DialResult resultdial = call.Dial();
                Call z = resultdial.Call;
                z.PlayTTS("We are testing");
                if (resultdial.Successful)
                {
                    z.PlayTTS("Connecting to new call");
                    Thread.Sleep(1000);

                    ConnectResult resultConnect = z.Connect(new List<List<CallDevice>>
                        {
                            new List<CallDevice>
                            {
                                new CallDevice
                                {
                                    Type = CallDevice.DeviceType.phone,
                                    Parameters = new CallDevice.PhoneParams
                                    {
                                        ToNumber = Globals.SW_num,
                                        FromNumber = cus_num,
                                        Timeout = 30,
                                    }
                                }
                            },
                        }
                    );


                    if (!resultConnect.Successful) return;
                    Call y = resultConnect.Call;
                    y.Hangup();
                    call.Hangup();
                    Welcome(z, Client);
                }
            }
            else if (msg_body == "n")
            {
                Client.Messaging.Send("test", cus_num, Globals.SW_num, new SendSource("Feelings are hurt"));
            }

            else
            {
                Client.Messaging.Send("test", cus_num, Globals.SW_num, new SendSource("Wrong input"));
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
