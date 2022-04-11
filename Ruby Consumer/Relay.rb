#frozen_string_literal: true
#
require "signalwire"
$LOAD_PATH.unshift(File.dirname(__FILE__) + '/../lib')
%w[
  bundler/setup
  signalwire
].each { |f| require f }
SIGNALWIRE_PROJECT_KEY = 'Project Here'
SIGNALWIRE_TOKEN = 'Token Here'

Signalwire::Logger.logger.level = ::Logger::DEBUG

class MyConsumer < Signalwire::Relay::Consumer
  contexts ['test']

  def on_incoming_call(call)
    call.answer
    call.play_tts text: 'welcome to signalwire'
    play_action = call.play_audio!('https://cdn.signalwire.com/default-music/welcome.mp3')
    sleep (5)
    play_action.stop
    sleep (1)
    connected = call.connect [
      [{ type: 'phone', params: { to_number: '+1XXXXXXXXXX',from_number: '+1YYYYYYYYYY', timeout: 30 } }],
                             ]
        if connected.successful
          # connected.call is the remote leg connected with yours.
          connected.call.wait_for_ended
        end
        call.hangup
      end

      def teardown
        puts "this is an example of teardown"
      end
      end

MyConsumer.new(project: SIGNALWIRE_PROJECT_KEY, token: SIGNALWIRE_TOKEN).run
