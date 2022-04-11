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
    call.play_tts text: 'the quick brown fox jumps over the lazy dog'

    call.hangup
  end

  def teardown
    puts "this is an example of teardown"
  end
end

MyConsumer.new(project: 'Project Here', token: 'Token Here').run
