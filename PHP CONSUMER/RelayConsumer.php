<?php

require dirname(__FILE__) . '/vendor/autoload.php';

use Generator as Coroutine;
use SignalWire\Relay\Consumer;

class CustomConsumer extends Consumer {
    public $project = 'PROJECT HERE';
    public $token = 'TOKEN HERE';
    public $contexts = ['test'];

    public function ready(): Coroutine {
        yield;
        // Consumer is successfully connected with Relay.
        // You can make calls or send messages here..
    }

    public function onIncomingCall($call): Coroutine {
        $result = yield $call->answer();
        if ($result->isSuccessful()) {
            $call->playTTS(['text' => 'Welcome to SignalWire!']);
            $call->playAudioAsync('https://cdn.signalwire.com/default-music/welcome.mp3')->done(function($action) use ($call) {
                sleep(5);
                $action->stop();
                $devices = [
                    [ "type" => "phone", "to" => "+1XXXXXXXXXX", "timeout" => 30 ],
                ];
                echo $action->getControlId();
                $call->connect($devices)->done(function($connectResult) {
                    if ($connectResult->isSuccessful()) {
                        $remoteCall = $connectResult->getCall();
                    }
                });
            });
        }
    }
}

$consumer = new CustomConsumer();
$consumer->run();
