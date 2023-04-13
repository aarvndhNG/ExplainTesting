const fs = require('fs');

const configPath = './config.json';

let config = {
  responses: [
    { trigger: 'hello', response: 'Hello there!' },
    { trigger: 'how are you', response: "I'm doing great, thanks for asking." },
    { trigger: 'help', response: 'How can I help you?' }
  ],
  chatAssistantPrefix: '[Chat Assistant]',
  chatAssistantColor: 'Orange'
};

if (fs.existsSync(configPath)) {
  const rawData = fs.readFileSync(configPath);
  const jsonData = JSON.parse(rawData);
  config = { ...config, ...jsonData };
} else {
  fs.writeFileSync(configPath, JSON.stringify(config, null, 2));
}

function onChat(player, message, channel) {
  const lowerCaseMessage = message.toLowerCase();
  const response = config.responses.find((res) => lowerCaseMessage.includes(res.trigger))?.response;
  if (response) {
    const chatAssistantMessage = `${config.chatAssistantPrefix} ${response}`;
    const chatAssistantColor = Color[config.chatAssistantColor];
    TShock.Utils.Broadcast(chatAssistantMessage, chatAssistantColor);
  }
}

module.exports = {
  name: 'ChatAssistant',
  version: '1.0.0',
  author: 'Your Name',
  description: 'A chat assistant plugin for TShock',
  init() {
    TShockAPI.Hooks.ServerHooks.Chat.Register(this, onChat);
  }
};
