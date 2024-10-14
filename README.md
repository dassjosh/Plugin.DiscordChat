## Features

* Allows for bidirectional chatting between a game server and discord channel.
* Any Discord linked players who chat in that discord chat channel will have their chat displayed on the game server
* All chat plugins that use the OnUserChat hook are supported as well as default game chat

## Discord Link
This plugin supports Discord Link provided by the Discord Extension. This plugin will work with any plugin that provides linked player data through Discord Link.

## Getting Your Bot Token
[Click Here to learn how to get an Discord Bot Token](https://umod.org/extensions/discord#getting-your-api-key)

## Configuration

```json
{
  "Discord Bot Token": "",
  "Chat Settings": {
    "Chat Channel ID": "",
    "Team Channel ID": "",
    "Cards Channel ID": "",
    "Clans Channel ID": "",
    "Replace Discord User Message With Bot Message": true,
    "Send Messages From Server Chat To Discord Channel": true,
    "Send Messages From Discord Channel To Server Chat": true,
    "Add Discord Tag To In Game Messages When Sent From Discord": "[#5f79d6][Discord][/#]",
    "Allow plugins to process Discord to Server Chat Messages": true,
    "Text Replacements": {
    },
    "Unlinked Settings": {
      "Allow Unlinked Players To Chat With Server": true,
      "Steam Icon ID": 76561199144296099
    },
    "Message Filter Settings": {
      "Ignore messages from users in this list (Discord ID)": [],
      "Ignore messages from users in this role (Role ID)": [],
      "Ignored Prefixes": []
    }
  },
  "Player State Settings": {
    "Player State Channel ID": "460919879076413444",
    "Show Admins": true,
    "Send Connecting Message": true,
    "Send Connected Message": true,
    "Send Disconnected Message": true
  },
  "Server State Settings": {
    "Server State Channel ID": "460919879076413444",
    "Send Booting Message": true,
    "Send Online Message": true,
    "Send Shutdown Message": true
  },
  "Plugin Support": {
    "AdminChat Settings": {
      "Enable AdminChat Plugin Support": false,
      "Chat Channel ID": "",
      "Chat Prefix": "@",
      "Replace Discord Message With Bot": true
    },
    "AntiSpam Settings": {
      "Use AntiSpam On Player Names": false,
      "Use AntiSpam On Server Messages": false,
      "Use AntiSpam On Chat Messages": false,
      "Use AntiSpam On Plugin Messages": false,
      "Use AntiSpam On Team Messages": false,
      "Use AntiSpam On Card Messages": false,
      "Use AntiSpam On Clan Messages": false
    },
    "BetterChat Settings": {
      "Max BetterChat Tags To Show When Sent From Discord": 3
    },
    "BetterChatMute Settings": {
      "Ignore Muted Players": true
    },
    "ChatTranslator Settings": {
      "Enable Chat Translator": false,
      "Use ChatTranslator On Server Messages": false,
      "Use ChatTranslator On Chat Messages": false,
      "Use ChatTranslator On Plugin Messages": false,
      "Use ChatTranslator On Team Messages": false,
      "Use ChatTranslator On Card Messages": false,
      "Use ChatTranslator On Clan Messages": false,
      "Discord Server Chat Language": "en"
    },
    "Clan Settings": {
      "Clans Chat Channel ID": "",
      "Alliance Chat Channel ID": ""
    },
    "UFilter Settings": {
      "Use UFilter On Player Names": false,
      "Use UFilter On Server Messages": false,
      "Use UFilter On Discord Messages": false,
      "Use UFilter On Plugin Messages": false,
      "Use UFilter On Team Messages": false,
      "Use UFilter On Card Messages": false,
      "Use UFilter On Clan Messages": false,
      "Replacement Character": "＊"
    }
  },
  "Discord Extension Log Level (Verbose, Debug, Info, Warning, Error, Exception, Off)": "Info"
}
```

## Server Emojis
Discord Chat supports server emojis. If you wish to install a pre-made collection of emojis you can find them [Here](https://github.com/dassjosh/Plugin.DiscordChat/raw/refs/heads/main/serveremoji.zip). To learn how to install server emojis check out the [Server Emoji Documentation](https://wiki.facepunch.com/rust/server-custom-emojis)

### Note:
To disable channel sending functionality leave that channel blank.

## Localization
```json
{
  "V3.Discord.Player.Connecting": ":yellow_circle: {timestamp.now.shorttime} {ip.country.emoji} **{discordchat.player.name}** is connecting",
  "V3.Discord.Player.Connected": ":white_check_mark: {timestamp.now.shorttime} {player.country.emoji} **{discordchat.player.name}** has joined.",
  "V3.Discord.Player.Disconnected": ":x: {timestamp.now.shorttime} **{discordchat.player.name}** has disconnected. ({discordchat.disconnect.reason})",
  "V3.Discord.Chat.Server": ":desktop: {timestamp.now.shorttime} **{discordchat.player.name}**: {discordchat.player.message}",
  "V3.Discord.Chat.LinkedMessage": ":speech_left: {timestamp.now.shorttime} **{discordchat.player.name}**: {discordchat.player.message}",
  "V3.Discord.Chat.UnlinkedMessage": ":chains: {timestamp.now.shorttime} {user.mention}: {discordchat.player.message}",
  "V3.Discord.Chat.PlayerName": "{player.name:clan}",
  "V3.Discord.Team.Message": ":busts_in_silhouette: {timestamp.now.shorttime} **{discordchat.player.name}**: {discordchat.player.message}",
  "V3.Discord.Cards.Message": ":black_joker: {timestamp.now.shorttime} **{discordchat.player.name}**: {discordchat.player.message}",
  "V3.Discord.Clans.Message": ":shield: {timestamp.now.shorttime} **{discordchat.player.name}**: {discordchat.player.message}",
  "V3.Discord.AdminChat.ServerMessage": ":mechanic: {timestamp.now.shorttime} **{discordchat.player.name}**: {discordchat.player.message}",
  "V3.Discord.AdminChat.DiscordMessage": ":mechanic: {timestamp.now.shorttime} **{discordchat.player.name}**: {discordchat.player.message}",
  "V3.Discord.PluginClans.ClanMessage": "{timestamp.now.shorttime} [Clan] **{discordchat.player.name}**: {discordchat.player.message}",
  "V3.Discord.PluginClans.AllianceMessage": "{timestamp.now.shorttime} [Alliance] **{discordchat.player.name}**: {discordchat.player.message}",
  "V3.Server.UnlinkedMessage": "{discordchat.discord.tag} [#5f79d6]{member.name}[/#]: {discordchat.player.message}",
  "V3.Server.LinkedMessage": "{discordchat.discord.tag} [#5f79d6]{discordchat.player.name}[/#]: {discordchat.player.message}"
}
```