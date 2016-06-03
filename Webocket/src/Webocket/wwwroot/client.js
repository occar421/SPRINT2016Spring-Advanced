'use strict';

var host = window.location.host;
var ws = new WebSocket('ws://' + host + '/');

$(function () {
  $('form').submit(function(){
    var $this = $(this);
    // ws.onopen = function() {
    //   console.log('sent message: %s', $('#m').val());
    // };
    ws.send($('#m').val());
    $('#m').val('');
    return false;
  });
  ws.onmessage = function(msg){
  	var returnObject = JSON.parse(msg.data);
  	var content = returnObject.data.split('\n').join('<br />');
    $('#messages').append($('<li>')).append($('<span id="clientId" class="' + (returnObject.isBot ? 'bot' : 'user') + '">').text(returnObject.id)).append($('<span id="clientMessage">' + content + '<span />'));
  };
  ws.onerror = function(err){
    console.log("err", err);
  };
  ws.onclose = function close() {
    console.log('disconnected');
  };
});
