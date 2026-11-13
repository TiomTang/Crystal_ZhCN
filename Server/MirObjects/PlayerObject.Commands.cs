using System.Drawing;
using C = ClientPackets;
using Server.MirDatabase;
using Server.MirEnvir;
using Server.MirNetwork;
using S = ServerPackets;
using System.Text.RegularExpressions;
using Timer = Server.MirEnvir.Timer;

namespace Server.MirObjects
{
    public partial class PlayerObject
    {
        /// <summary>Handles @KILL.</summary>
        internal void Cmd_KILL(string[] parts)
        {
            PlayerObject player;

                        if (!IsGM) return;

                        if (parts.Length >= 2)
                        {
                            player = Envir.GetPlayer(parts[1]);

                            if (player == null)
                            {
                                ReceiveChat(GameLanguage.Get("Could not find {0}.", parts[1]), ChatType.System);
                                return;
                            }

                            if (!player.GMNeverDie)
                            {
                                player.Die();

                                Helpers.ChatSystem.SystemMessage(chatMessage: GameLanguage.Get("{0} was totally killed by GM: {1}", player.Name, Name));
                                Helpers.ChatSystem.SystemMessage(chatMessage: GameLanguage.Get("{0} was totally killed by GM: {1}", player.Name, Name));
                            }
                        }
                        else
                        {
                            if (!CurrentMap.ValidPoint(Front)) return;

                            Cell cell = CurrentMap.GetCell(Front);

                            if (cell == null || cell.Objects == null) return;

                            for (int i = 0; i < cell.Objects.Count; i++)
                            {
                                MapObject ob = cell.Objects[i];

                                switch (ob.Race)
                                {
                                    case ObjectType.Player:
                                    case ObjectType.Monster:
                                        if (ob.Dead) continue;
                                        ob.EXPOwner = this;
                                        ob.ExpireTime = Envir.Time + MonsterObject.EXPOwnerDelay;
                                        ob.Die();

                                        if (ob is PlayerObject killedPlayer)
                                        {
                                            Helpers.ChatSystem.SystemMessage(chatMessage: GameLanguage.Get("{0} was totally killed by GM: {1}", killedPlayer.Name, Name));
                                        }
                                        break;

                                    default:
                                        continue;
                                }
                            }
                        }
                        return;
        }
        /// <summary>Handles @CHANGEGENDER.</summary>
        internal void Cmd_CHANGEGENDER(string[] parts)
        {
            CharacterInfo data;

                        if (!IsGM && !Settings.TestServer) return;

                        data = parts.Length < 2 ? Info : Envir.GetCharacterInfo(parts[1]);

                        if (data == null) return;

                        switch (data.Gender)
                        {
                            case MirGender.Male:
                                data.Gender = MirGender.Female;
                                break;
                            case MirGender.Female:
                                data.Gender = MirGender.Male;
                                break;
                        }

                        ReceiveChat(GameLanguage.Get("Player {0} has been changed to {1}", data.Name, data.Gender), ChatType.System);
                        MessageQueue.Enqueue(GameLanguage.Get("Player {0} has been changed to {1} by {2}", data.Name, data.Gender, Name));

                        Helpers.ChatSystem.SystemMessage(chatMessage: GameLanguage.Get("{0} had gender change to {1} by GM: {2}", data.Player.Name, data.Gender.ToString(), Name));

                        if (data.Player != null)
                            data.Player.Connection.LogOut();

                        return;
        }
        /// <summary>Handles @LEVEL.</summary>
        internal void Cmd_LEVEL(string[] parts)
        {
            PlayerObject player;

                        if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;

                        ushort level;
                        ushort old;
                        if (parts.Length >= 3)
                        {
                            if (!IsGM) return;

                            if (ushort.TryParse(parts[2], out level))
                            {
                                if (level == 0) return;
                                player = Envir.GetPlayer(parts[1]);
                                if (player == null) return;
                                old = player.Level;
                                player.Level = level;
                                player.LevelUp();

                                ReceiveChat(GameLanguage.Get("Player {0} has been Levelled {1} -> {2}.", player.Name, old, player.Level), ChatType.System);
                                MessageQueue.Enqueue(GameLanguage.Get("Player {0} has been Leveled {1} -> {2} by {3}", player.Name, old, player.Level, Name));
                                Helpers.ChatSystem.SystemMessage(chatMessage: GameLanguage.Get("Player {0} has been Levelled: {1} -> {2} by GM: {3}", player.Name, old, player.Level, Name));

                                return;
                            }
                        }
                        else
                        {
                            if (parts[1] == "-1")
                            {
                                parts[1] = ushort.MaxValue.ToString();
                            }

                            if (ushort.TryParse(parts[1], out level))
                            {
                                if (level == 0) return;
                                old = Level;
                                Level = level;
                                LevelUp();

                                ReceiveChat(string.Format("{0} {1} -> {2}.", GameLanguage.Get("Congratulations! You have leveled up. Your HP and MP have been restored."), old, Level), ChatType.System);
                                MessageQueue.Enqueue(GameLanguage.Get("Player {0} has been Leveled {1} -> {2} by {3}", Name, old, Level, Name));
                                return;
                            }
                        }

                        ReceiveChat(GameLanguage.Get("Could not level player"), ChatType.System);
                        return;
        }
        /// <summary>Handles @LEVELHERO.</summary>
        internal void Cmd_LEVELHERO(string[] parts)
        {
            ushort level;
            ushort old;
            PlayerObject player;

                        if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;

                        if (parts.Length >= 3)
                        {
                            if (!IsGM) return;

                            if (ushort.TryParse(parts[2], out level))
                            {
                                if (level == 0) return;
                                player = Envir.GetPlayer(parts[1]);
                                if (player == null) return;
                                HeroObject hero = player.GetHero();
                                if (hero == null) return;
                                old = hero.Level;
                                hero.Level = level;
                                hero.LevelUp();

                                ReceiveChat(GameLanguage.Get("Player {0}'s hero has been Levelled {1} -> {2}.", player.Name, old, hero.Level), ChatType.System);
                                MessageQueue.Enqueue(GameLanguage.Get("Player {0}'s hero has been Levelled {1} -> {2} by {3}", player.Name, old, hero.Level, Name));
                                Helpers.ChatSystem.SystemMessage(chatMessage: GameLanguage.Get("Player {0}'s hero has been Levelled: {1} -> {2} by GM: {3}", player.Name, old, hero.Level, Name));
                                return;
                            }
                        }
                        else
                        {
                            HeroObject hero = GetHero();
                            if (hero == null) return;

                            if (parts[1] == "-1")
                            {
                                parts[1] = ushort.MaxValue.ToString();
                            }

                            if (ushort.TryParse(parts[1], out level))
                            {
                                if (level == 0) return;
                                old = hero.Level;
                                hero.Level = level;
                                hero.LevelUp();

                                ReceiveChat(string.Format("{0} {1} -> {2}.", GameLanguage.Get("Congratulations! You have leveled up. Your HP and MP have been restored."), old, hero.Level), ChatType.System);
                                MessageQueue.Enqueue(GameLanguage.Get("Player {0}'s hero has been Leveled {1} -> {2} by {3}", Name, old, hero.Level, Name));
                                return;
                            }
                        }

                        ReceiveChat(GameLanguage.Get("Could not level player"), ChatType.System);
                        return;
        }
        /// <summary>Handles @MAKE.</summary>
        internal void Cmd_MAKE(string[] parts)
        {
            UserItem item;

                        {
                            if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;

                            ItemInfo iInfo;
                            int itemIndex = 0;

                            if (Int32.TryParse(parts[1], out itemIndex))
                            {
                                iInfo = Envir.GetItemInfo(itemIndex);
                            }
                            else
                            {
                                iInfo = Envir.GetItemInfo(parts[1]);
                            }

                            if (iInfo == null) return;

                            ushort itemCount = 1;
                            if (parts.Length >= 3 && !ushort.TryParse(parts[2], out itemCount))
                                itemCount = 1;

                            var tempCount = itemCount;

                            while (itemCount > 0)
                            {
                                if (iInfo.StackSize >= itemCount)
                                {
                                    item = Envir.CreateDropItem(iInfo);
                                    item.GMMade = true;
                                    item.Count = itemCount;

                                    if (CanGainItem(item)) GainItem(item);

                                    return;
                                }
                                item = Envir.CreateDropItem(iInfo);
                                item.GMMade = true;
                                item.Count = iInfo.StackSize;
                                itemCount -= iInfo.StackSize;

                                if (!CanGainItem(item)) return;
                                GainItem(item);
                            }

                            ReceiveChat(GameLanguage.Get("{0} x{1} has been created.", iInfo.FriendlyName, tempCount), ChatType.System);
                            MessageQueue.Enqueue(GameLanguage.Get("Player {0} has attempted to Create {1} x{2}", Name, iInfo.FriendlyName, tempCount));
                        }
                        return;
        }
        /// <summary>Handles @CLEARBAG.</summary>
        internal void Cmd_CLEARBAG(string[] parts)
        {
            PlayerObject player;
            UserItem item;

                        if (!IsGM && !Settings.TestServer) return;
                        player = this;

                        if (parts.Length >= 2)
                            player = Envir.GetPlayer(parts[1]);

                        if (player == null) return;
                        for (int i = 0; i < player.Info.Inventory.Length; i++)
                        {
                            item = player.Info.Inventory[i];
                            if (item == null) continue;

                            player.Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });
                            player.Info.Inventory[i] = null;
                        }
                        player.RefreshStats();
                        return;
        }
        /// <summary>Handles @SUPERMAN.</summary>
        internal void Cmd_SUPERMAN(string[] parts)
        {
            string hintstring;

                        if (!IsGM && !Settings.TestServer) return;

                        GMNeverDie = !GMNeverDie;

                        hintstring = GMNeverDie ? GameLanguage.Get("Invincible Mode.") : GameLanguage.Get("Normal Mode.");
                        ReceiveChat(hintstring, ChatType.Hint);
                        UpdateGMBuff();
                        return;
        }
        /// <summary>Handles @GAMEMASTER.</summary>
        internal void Cmd_GAMEMASTER(string[] parts)
        {
            string hintstring;

                        if (!IsGM && !Settings.TestServer) return;

                        GMGameMaster = !GMGameMaster;

                        hintstring = GMGameMaster ? GameLanguage.Get("GameMaster Mode.") : GameLanguage.Get("Normal Mode.");
                        ReceiveChat(hintstring, ChatType.Hint);
                        UpdateGMBuff();
                        return;
        }
        /// <summary>Handles @OBSERVER.</summary>
        internal void Cmd_OBSERVER(string[] parts)
        {
            string hintstring;

                        if (!IsGM) return;
                        Observer = !Observer;

                        hintstring = Observer ? GameLanguage.Get("Observer Mode.") : GameLanguage.Get("Normal Mode.");
                        ReceiveChat(hintstring, ChatType.Hint);
                        UpdateGMBuff();
                        return;
        }
        /// <summary>Handles @RECALL.</summary>
        internal void Cmd_RECALL(string[] parts)
        {
            PlayerObject player;

                        if (!IsGM) return;

                        if (parts.Length < 2) return;
                        player = Envir.GetPlayer(parts[1]);

                        if (player == null) return;

                        player.Teleport(CurrentMap, Front);
                        return;
        }
        /// <summary>Handles @BACKUPPLAYER.</summary>
        internal void Cmd_BACKUPPLAYER(string[] parts)
        {

                        {
                            if (!IsGM || parts.Length < 2) return;

                            var info = Envir.GetCharacterInfo(parts[1]);

                            if (info == null)
                            {
                                ReceiveChat(GameLanguage.Get("Player {0} was not found", parts[1]), ChatType.System);
                                return;
                            }

                            Envir.SaveArchivedCharacter(info);

                            ReceiveChat(GameLanguage.Get("Player {0} has been backed up", info.Name), ChatType.System);
                            MessageQueue.Enqueue(GameLanguage.Get("Player {0} has been backed up by {1}", info.Name, Name));
                        }
                        return;
        }
        /// <summary>Handles @ARCHIVEPLAYER.</summary>
        internal void Cmd_ARCHIVEPLAYER(string[] parts)
        {
            CharacterInfo data;

                        {
                            if (!IsGM || parts.Length < 2) return;

                            data = Envir.GetCharacterInfo(parts[1]);

                            if (data == null)
                            {
                                ReceiveChat(GameLanguage.Get("Player {0} was not found", parts[1]), ChatType.System);
                                return;
                            }

                            if (data == Info)
                            {
                                ReceiveChat(GameLanguage.Get("Cannot archive the player you are on"), ChatType.System);
                                return;
                            }

                            var account = Envir.GetAccountByCharacter(parts[1]);

                            if (account == null)
                            {
                                ReceiveChat(GameLanguage.Get("Player {0} was not found in any account", parts[1]), ChatType.System);
                                return;
                            }

                            Envir.SaveArchivedCharacter(data);

                            Envir.CharacterList.Remove(data);
                            account.Characters.Remove(data);

                            ReceiveChat(GameLanguage.Get("Player {0} has been archived", data.Name), ChatType.System);
                            MessageQueue.Enqueue(GameLanguage.Get("Player {0} has been archived by {1}", data.Name, Name));
                        }
                        return;
        }
        /// <summary>Handles @LOADPLAYER.</summary>
        internal void Cmd_LOADPLAYER(string[] parts)
        {

                        {
                            if (!IsGM) return;

                            if (parts.Length < 2) return;

                            var bak = Envir.GetArchivedCharacter(parts[1]);

                            if (bak == null)
                            {
                                ReceiveChat(GameLanguage.Get("Player {0} could not be loaded. Try specifying the full archive filename", parts[1]), ChatType.System);
                                return;
                            }

                            var info = Envir.GetCharacterInfo(bak.Name);

                            if (info == null)
                            {
                                ReceiveChat(GameLanguage.Get("Player {0} was not found", parts[1]), ChatType.System);
                                return;
                            }

                            if (info.Index != bak.Index)
                            {
                                ReceiveChat(GameLanguage.Get("Cannot load this player due to mismatching ID's"), ChatType.System);
                                return;
                            }

                            info = bak;

                            ReceiveChat(GameLanguage.Get("Player {0} has been loaded", info.Name), ChatType.System);
                            MessageQueue.Enqueue(GameLanguage.Get("Player {0} has been loaded by {1}", info.Name, Name));
                        }
                        return;
        }
        /// <summary>Handles @RESTOREPLAYER.</summary>
        internal void Cmd_RESTOREPLAYER(string[] parts)
        {
            CharacterInfo data;

                        {
                            if (!IsGM || parts.Length < 2) return;

                            AccountInfo account = null;

                            if (parts.Length > 2)
                            {
                                if (!Envir.AccountExists(parts[2]))
                                {
                                    ReceiveChat(GameLanguage.Get("Account {0} was not found", parts[2]), ChatType.System);
                                    return;
                                }

                                account = Envir.GetAccount(parts[2]);

                                if (account.Characters.Count >= Globals.MaxCharacterCount)
                                {
                                    ReceiveChat(GameLanguage.Get("Account {0} already has {1} characters", parts[2], Globals.MaxCharacterCount), ChatType.System);
                                    return;
                                }
                            }

                            data = Envir.GetCharacterInfo(parts[1]);

                            if (data == null)
                            {
                                if (account != null)
                                {
                                    data = Envir.GetArchivedCharacter(parts[1]);

                                    if (data == null)
                                    {
                                        ReceiveChat(GameLanguage.Get("Player {0} could not be restored. Try specifying the full archive filename", parts[1]), ChatType.System);
                                        return;
                                    }

                                    data.AccountInfo = account;

                                    account.Characters.Add(data);
                                    Envir.CharacterList.Add(data);

                                    data.Deleted = false;
                                    data.DeleteDate = DateTime.MinValue;

                                    data.LastLoginDate = Envir.Now;
                                }
                                else
                                {
                                    ReceiveChat(GameLanguage.Get("Player {0} was not found", parts[1]), ChatType.System);
                                    return;
                                }
                            }
                            else
                            {
                                if (!data.Deleted) return;
                                data.Deleted = false;
                                data.DeleteDate = DateTime.MinValue;
                            }

                            ReceiveChat(GameLanguage.Get("Player {0} has been restored by {1}", data.Name, Name), ChatType.System);
                            MessageQueue.Enqueue(GameLanguage.Get("Player {0} has been restored by {1}", data.Name, Name));
                        }
                        return;
        }
        /// <summary>Handles @MOVE.</summary>
        internal void Cmd_MOVE(string[] parts)
        {

                        if (!IsGM && !SpecialMode.HasFlag(SpecialItemMode.Teleport) && !Settings.TestServer) return;
                        if (!IsGM && CurrentMap.Info.NoPosition)
                        {
                            ReceiveChat((GameLanguage.Get("You cannot position move on this map")), ChatType.System);
                            return;
                        }
                        if (Envir.Time < LastTeleportTime)
                        {
                            ReceiveChat(GameLanguage.Get("You cannot teleport for another {0} seconds", (LastTeleportTime - Envir.Time) / 1000), ChatType.System);
                            return;
                        }

                        int x, y;

                        if (parts.Length <= 2 || !int.TryParse(parts[1], out x) || !int.TryParse(parts[2], out y))
                        {
                            if (!IsGM)
                                LastTeleportTime = Envir.Time + 10000;
                            TeleportRandom(200, 0);
                            return;
                        }
                        if (!IsGM)
                            LastTeleportTime = Envir.Time + 10000;
                        Teleport(CurrentMap, new Point(x, y));
                        return;
        }
        /// <summary>Handles @MAPMOVE.</summary>
        internal void Cmd_MAPMOVE(string[] parts)
        {
            int x;
            int y;

                        if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;
                        var instanceID = 1; x = 0; y = 0;

                        if (parts.Length == 3 || parts.Length == 5)
                            int.TryParse(parts[2], out instanceID);

                        if (instanceID < 1) instanceID = 1;

                        var map = Envir.GetMapByNameAndInstance(parts[1], instanceID);
                        if (map == null)
                        {
                            ReceiveChat((GameLanguage.Get("Map {0}:[{1}] could not be found", parts[1], instanceID)), ChatType.System);
                            return;
                        }

                        if (parts.Length == 4 || parts.Length == 5)
                        {
                            int.TryParse(parts[parts.Length - 2], out x);
                            int.TryParse(parts[parts.Length - 1], out y);
                        }

                        switch (parts.Length)
                        {
                            case 2:
                                ReceiveChat(TeleportRandom(200, 0, map) ? (GameLanguage.Get("Moved to Map {0}", map.Info.FileName)) :
                                    (GameLanguage.Get("Failed movement to Map {0}", map.Info.FileName)), ChatType.System);
                                break;
                            case 3:
                                ReceiveChat(TeleportRandom(200, 0, map) ? (GameLanguage.Get("Moved to Map {0}:[{1}]", map.Info.FileName, instanceID)) :
                                    (GameLanguage.Get("Failed movement to Map {0}:[{1}]", map.Info.FileName, instanceID)), ChatType.System);
                                break;
                            case 4:
                                ReceiveChat(Teleport(map, new Point(x, y)) ? (GameLanguage.Get("Moved to Map {0} at {1}:{2}", map.Info.FileName, x, y)) :
                                    (GameLanguage.Get("Failed movement to Map {0} at {1}:{2}", map.Info.FileName, x, y)), ChatType.System);
                                break;
                            case 5:
                                ReceiveChat(Teleport(map, new Point(x, y)) ? (GameLanguage.Get("Moved to Map {0}:[{1}] at {2}:{3}", map.Info.FileName, instanceID, x, y)) :
                                    (GameLanguage.Get("Failed movement to Map {0}:[{1}] at {2}:{3}", map.Info.FileName, instanceID, x, y)), ChatType.System);
                                break;
                        }
                        return;
        }
        /// <summary>Handles @GOTO.</summary>
        internal void Cmd_GOTO(string[] parts)
        {
            PlayerObject player;

                        if (!IsGM) return;

                        if (parts.Length < 2) return;
                        player = Envir.GetPlayer(parts[1]);

                        if (player == null) return;

                        Teleport(player.CurrentMap, player.CurrentLocation);
                        return;
        }
        /// <summary>Handles @MOB.</summary>
        internal void Cmd_MOB(string[] parts)
        {
            List<int> conquestAIs = new List<int> { 72, 73, 80, 81, 82 };

                        if (!IsGM && !Settings.TestServer) return;
                        if (parts.Length < 2)
                        {
                            ReceiveChat(GameLanguage.Get("Not enough parameters to spawn monster"), ChatType.System);
                            return;
                        }

                        MonsterInfo mInfo = null;
                        int monsterIndex = 0;

                        if (Int32.TryParse(parts[1], out monsterIndex))
                        {
                            mInfo = Envir.GetMonsterInfo(monsterIndex, false);
                        }
                        else
                        {
                            mInfo = Envir.GetMonsterInfo(parts[1]);
                        }

                        if (mInfo == null)
                        {
                            ReceiveChat((GameLanguage.Get("Monster {0} does not exist", parts[1])), ChatType.System);
                            return;
                        }

                        if (conquestAIs.Contains(mInfo.AI))
                        {
                            ReceiveChat(GameLanguage.Get("Cannot spawn conquest item: {0}", mInfo.Name), ChatType.System);
                            return;
                        }

                        uint count = 1;
                        if (parts.Length >= 3 && IsGM)
                            if (!uint.TryParse(parts[2], out count)) count = 1;
                        int spread = 0;
                        if (parts.Length >= 4)
                            int.TryParse(parts[3], out spread);

                        for (int i = 0; i < count; i++)
                        {
                            MonsterObject monster = MonsterObject.GetMonster(mInfo);
                            if (monster == null)
                            {
                                return;
                            }

                            if (monster is IntelligentCreatureObject)
                            {
                                ReceiveChat(GameLanguage.Get("Cannot spawn an IntelligentCreatureObject."), ChatType.System);
                                return;
                            }

                            monster.GMMade = true;

                            if (spread == 0)
                                monster.Spawn(CurrentMap, Front);
                            else
                                for (int _ = 0; _ < 20; _++)
                                    if (monster.Spawn(CurrentMap, CurrentLocation.Add(Envir.Random.Next(-spread, spread + 1), Envir.Random.Next(-spread, spread + 1))))
                                        break;
                        }

                        ReceiveChat((GameLanguage.Get("Monster {0} x{1} has been spawned.", mInfo.Name, count)), ChatType.System);
                        return;
        }
        /// <summary>Handles @RECALLMOB.</summary>
        internal void Cmd_RECALLMOB(string[] parts)
        {
            List<int> conquestAIs = new List<int> { 72, 73, 80, 81, 82 };
            uint count;

                        if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;

                        MonsterInfo mInfo2 = null;
                        int monsterIndex2 = 0;

                        if (Int32.TryParse(parts[1], out monsterIndex2))
                        {
                            mInfo2 = Envir.GetMonsterInfo(monsterIndex2, false);
                        }
                        else
                        {
                            mInfo2 = Envir.GetMonsterInfo(parts[1]);
                        }

                        if (mInfo2 == null) return;

                        count = 1;
                        byte petlevel = 0;

                        if (parts.Length > 2)
                            if (!uint.TryParse(parts[2], out count) || count > 50) count = 1;

                        if (parts.Length > 3)
                            if (!byte.TryParse(parts[3], out petlevel) || petlevel > 7) petlevel = 0;

                        if (!IsGM && (Pets.Count(t => !t.Dead && t.Race != ObjectType.Creature) >= Globals.MaxPets)) return;

                        for (int i = 0; i < count; i++)
                        {
                            MonsterObject monster = MonsterObject.GetMonster(mInfo2);

                            if (monster == null) return;

                            if (conquestAIs.Contains(monster.Info.AI))
                            {
                                ReceiveChat(GameLanguage.Get("Cannot spawn conquest item: {0}", monster.Name), ChatType.System);
                                return;
                            }
                            else if (monster is IntelligentCreatureObject)
                            {
                                ReceiveChat(GameLanguage.Get("Cannot spawn an IntelligentCreatureObject."), ChatType.System);
                                return;
                            }

                            monster.PetLevel = petlevel;
                            monster.Master = this;
                            monster.MaxPetLevel = 7;
                            monster.Direction = Direction;
                            monster.ActionTime = Envir.Time + 1000;
                            monster.Spawn(CurrentMap, Front);
                            Pets.Add(monster);
                        }

                        ReceiveChat((GameLanguage.Get("Pet {0} x{1} has been recalled.", mInfo2.Name, count)), ChatType.System);
                        return;
        }
        /// <summary>Handles @RELOADDROPS.</summary>
        internal void Cmd_RELOADDROPS(string[] parts)
        {

                        if (!IsGM) return;

                        Envir.ReloadDrops();

                        ReceiveChat(GameLanguage.Get("Drops Reloaded."), ChatType.Hint);
                        return;
        }
        /// <summary>Handles @RELOADNPCS.</summary>
        internal void Cmd_RELOADNPCS(string[] parts)
        {

                        if (!IsGM) return;

                        Envir.ReloadNPCs();

                        ReceiveChat(GameLanguage.Get("NPC Scripts Reloaded."), ChatType.Hint);
                        return;
        }
        /// <summary>Handles @CLEARIPBLOCKS.</summary>
        internal void Cmd_CLEARIPBLOCKS(string[] parts)
        {

                        if (!IsGM) return;

                        Envir.IPBlocks.Clear();
                        return;
        }
        /// <summary>Handles @GIVEGOLD.</summary>
        internal void Cmd_GIVEGOLD(string[] parts)
        {
            uint count;
            PlayerObject player;

                        if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;

                        player = this;

                        if (parts.Length > 2)
                        {
                            if (!IsGM) return;

                            if (!uint.TryParse(parts[2], out count)) return;
                            player = Envir.GetPlayer(parts[1]);

                            if (player == null)
                            {
                                ReceiveChat(GameLanguage.Get("Player {0} was not found", parts[1]), ChatType.System);
                                return;
                            }
                        }

                        else if (!uint.TryParse(parts[1], out count)) return;

                        if (count + player.Account.Gold >= uint.MaxValue)
                            count = uint.MaxValue - player.Account.Gold;

                        player.GainGold(count);

                        string goldMsg = GameLanguage.Get("Player {0} has been given {1} gold by GM: {2}", player.Name, count, Name);
                        MessageQueue.Enqueue(goldMsg);
                        Helpers.ChatSystem.SystemMessage(chatMessage: goldMsg);

                        return;
        }
        /// <summary>Handles @GIVEPEARLS.</summary>
        internal void Cmd_GIVEPEARLS(string[] parts)
        {
            uint count;
            PlayerObject player;

                        if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;

                        player = this;

                        if (parts.Length > 2)
                        {
                            if (!IsGM) return;

                            if (!uint.TryParse(parts[2], out count)) return;
                            player = Envir.GetPlayer(parts[1]);

                            if (player == null)
                            {
                                ReceiveChat(GameLanguage.Get("Player {0} was not found", parts[1]), ChatType.System);
                                return;
                            }
                        }

                        else if (!uint.TryParse(parts[1], out count)) return;

                        if (count + player.Info.PearlCount >= int.MaxValue)
                            count = (uint)(int.MaxValue - player.Info.PearlCount);

                        player.IntelligentCreatureGainPearls((int)count);

                        string pearlMsg = GameLanguage.Get("Player {0} has been given {1} pearl by GM: {2}", player.Name, count, Name);

                        MessageQueue.Enqueue(pearlMsg);
                        Helpers.ChatSystem.SystemMessage(chatMessage: pearlMsg);

                        return;
        }
        /// <summary>Handles @GIVECREDIT.</summary>
        internal void Cmd_GIVECREDIT(string[] parts)
        {
            uint count;
            PlayerObject player;

                        if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;

                        player = this;

                        if (parts.Length > 2)
                        {
                            if (!IsGM) return;

                            if (!uint.TryParse(parts[2], out count)) return;
                            player = Envir.GetPlayer(parts[1]);

                            if (player == null)
                            {
                                ReceiveChat(GameLanguage.Get("Player {0} was not found", parts[1]), ChatType.System);
                                return;
                            }
                        }

                        else if (!uint.TryParse(parts[1], out count)) return;

                        if (count + player.Account.Credit >= uint.MaxValue)
                            count = uint.MaxValue - player.Account.Credit;

                        player.GainCredit(count);

                        string creditMsg = GameLanguage.Get("Player {0} has been given {1} credit by GM: {2}", player.Name, count, Name);

                        MessageQueue.Enqueue(GameLanguage.Get("Player {0} has been given {1} credit", player.Name, count));
                        Helpers.ChatSystem.SystemMessage(chatMessage: creditMsg);

                        return;
        }
        /// <summary>Handles @GIVESKILL.</summary>
        internal void Cmd_GIVESKILL(string[] parts)
        {
            PlayerObject player;

                        if ((!IsGM && !Settings.TestServer) || parts.Length < 3) return;

                        byte spellLevel = 0;

                        player = this;
                        Spell skill;

                        if (!Enum.TryParse(parts.Length > 3 ? parts[2] : parts[1], true, out skill) || !Enum.IsDefined(skill)) return;

                        if (skill == Spell.None) return;

                        spellLevel = byte.TryParse(parts.Length > 3 ? parts[3] : parts[2], out spellLevel) ? Math.Min((byte)3, spellLevel) : (byte)0;

                        if (parts.Length > 3)
                        {
                            if (!IsGM) return;

                            player = Envir.GetPlayer(parts[1]);

                            if (player == null)
                            {
                                ReceiveChat(GameLanguage.Get("Player {0} was not found", parts[1]), ChatType.System);
                                return;
                            }
                        }

                        var magic = new UserMagic(skill) { Level = spellLevel };

                        if (player.Info.Magics.Any(e => e.Spell == skill))
                        {
                            player.Info.Magics.FirstOrDefault(e => e.Spell == skill).Level = spellLevel;

                            Enqueue(new S.MagicLeveled { ObjectID = ObjectID, Spell = magic.Spell, Level = magic.Level, Experience = 0 });

                            string skillChangeMsg = GameLanguage.Get("{0} Spell {1} changed to level {2} by GM: {3}", player.Name, skill.ToString(), spellLevel, Name);

                            player.ReceiveChat(GameLanguage.Get("Spell {0} changed to level {1}", skill.ToString(), spellLevel), ChatType.Hint);
                            Helpers.ChatSystem.SystemMessage(chatMessage: skillChangeMsg);

                            return;
                        }
                        else
                        {
                            player.ReceiveChat(GameLanguage.Get("You have learned {0} at level {1}", skill.ToString(), spellLevel), ChatType.Hint);

                            if (player != this)
                            {
                                ReceiveChat(GameLanguage.Get("{0} has learned {1} at level {2}", player.Name, skill.ToString(), spellLevel), ChatType.Hint);
                            }

                            string skillLearnedMg = GameLanguage.Get("{0} Spell {1} learnt and set to level {2} by GM: {3}", player.Name, skill.ToString(), spellLevel, Name);
                            Helpers.ChatSystem.SystemMessage(chatMessage: skillLearnedMg);

                            player.Info.Magics.Add(magic);
                        }

                        player.SendMagicInfo(magic);
                        player.RefreshStats();
                        return;
        }
        /// <summary>Handles @FIND.</summary>
        internal void Cmd_FIND(string[] parts)
        {
            PlayerObject player;

                        if (!IsGM && !SpecialMode.HasFlag(SpecialItemMode.Probe)) return;

                        if (Envir.Time < LastProbeTime)
                        {
                            ReceiveChat(GameLanguage.Get("You cannot search for another {0} seconds", (LastProbeTime - Envir.Time) / 1000), ChatType.System);
                            return;
                        }

                        if (parts.Length < 2) return;
                        player = Envir.GetPlayer(parts[1]);

                        if (player == null)
                        {
                            ReceiveChat(GameLanguage.Get("{0} is not online.", parts[1]), ChatType.System);
                            return;
                        }
                        if (player.CurrentMap == null) return;
                        if (!IsGM)
                            LastProbeTime = Envir.Time + 180000;
                        ReceiveChat((GameLanguage.Get("{0} is located at {1} ({2},{3})", player.Name, player.CurrentMap.Info.Title, player.CurrentLocation.X, player.CurrentLocation.Y)), ChatType.System);
                        return;
        }
        /// <summary>Handles @CREATEGUILD.</summary>
        internal void Cmd_CREATEGUILD(string[] parts)
        {
            PlayerObject player;

                        if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;

                        player = parts.Length < 3 ? this : Envir.GetPlayer(parts[1]);

                        if (player == null)
                        {
                            ReceiveChat(GameLanguage.Get("Player {0} was not found", parts[1]), ChatType.System);
                            return;
                        }

                        if (player.MyGuild != null)
                        {
                            ReceiveChat(GameLanguage.Get("Player {0} is already in a guild.", player.Name), ChatType.System);
                            return;
                        }

                        String gName = parts.Length < 3 ? parts[1] : parts[2];
                        if ((gName.Length < 3) || (gName.Length > 20))
                        {
                            ReceiveChat(GameLanguage.Get("Guildname is restricted to 3-20 characters."), ChatType.System);
                            return;
                        }

                        GuildObject guild = Envir.GetGuild(gName);
                        if (guild != null)
                        {
                            ReceiveChat(GameLanguage.Get("Guild {0} already exists.", gName), ChatType.System);
                            return;
                        }

                        player.CanCreateGuild = true;
                        if (player.CreateGuild(gName))
                        {
                            ReceiveChat(GameLanguage.Get("Successfully created guild {0}", gName), ChatType.System);
                        }
                        else
                        {
                            ReceiveChat(GameLanguage.Get("Failed to create guild"), ChatType.System);
                        }

                        player.CanCreateGuild = false;
                        return;
        }
        /// <summary>Handles @TRIGGER.</summary>
        internal void Cmd_TRIGGER(string[] parts)
        {
            PlayerObject player;

                        if (!IsGM) return;
                        if (parts.Length < 2) return;

                        if (parts.Length >= 3)
                        {
                            player = Envir.GetPlayer(parts[2]);

                            if (player == null)
                            {
                                ReceiveChat(GameLanguage.Get("Player {0} was not found", parts[2]), ChatType.System);
                                return;
                            }

                            player.CallDefaultNPC(DefaultNPCType.Trigger, parts[1]);
                            return;
                        }

                        foreach (var pl in Envir.Players)
                        {
                            pl.CallDefaultNPC(DefaultNPCType.Trigger, parts[1]);
                        }

                        return;
        }
        /// <summary>Handles @SETFLAG.</summary>
        internal void Cmd_SETFLAG(string[] parts)
        {

                        if (!IsGM && !Settings.TestServer) return;

                        if (parts.Length < 2) return;

                        int tempInt = 0;

                        if (!int.TryParse(parts[1], out tempInt)) return;

                        if (tempInt > Info.Flags.Length - 1) return;

                        Info.Flags[tempInt] = !Info.Flags[tempInt];

                        for (int f = CurrentMap.NPCs.Count - 1; f >= 0; f--)
                        {
                            if (Functions.InRange(CurrentMap.NPCs[f].CurrentLocation, CurrentLocation, Globals.DataRange))
                                CurrentMap.NPCs[f].CheckVisible(this);
                        }

                        return;
        }
        /// <summary>Handles @LISTFLAGS.</summary>
        internal void Cmd_LISTFLAGS(string[] parts)
        {

                        if (!IsGM && !Settings.TestServer) return;

                        for (int i = 0; i < Info.Flags.Length; i++)
                        {
                            if (Info.Flags[i] == false) continue;

                            ReceiveChat(GameLanguage.Get("Flag {0}", i), ChatType.Hint);
                        }
                        return;
        }
        /// <summary>Handles @CLEARFLAGS.</summary>
        internal void Cmd_CLEARFLAGS(string[] parts)
        {
            PlayerObject player;

                        if (!IsGM && !Settings.TestServer) return;

                        player = parts.Length > 1 && IsGM ? Envir.GetPlayer(parts[1]) : this;

                        if (player == null)
                        {
                            ReceiveChat(GameLanguage.Get("{0} is not online.", parts[1]), ChatType.System);
                            return;
                        }

                        for (int i = 0; i < player.Info.Flags.Length; i++)
                        {
                            player.Info.Flags[i] = false;
                        }
                        return;
        }
        /// <summary>Handles @CLEARMOB.</summary>
        internal void Cmd_CLEARMOB(string[] parts)
        {
            Map map;

                        if (!IsGM) return;

                        if (parts.Length > 1)
                        {
                            map = Envir.GetMapByNameAndInstance(parts[1]);

                            if (map == null) return;

                        }
                        else
                        {
                            map = CurrentMap;
                        }

                        foreach (var cell in map.Cells)
                        {
                            if (cell == null || cell.Objects == null) continue;

                            int obCount = cell.Objects.Count();

                            for (int m = 0; m < obCount; m++)
                            {
                                MapObject ob = cell.Objects[m];

                                if (ob.Race != ObjectType.Monster) continue;
                                if (ob.Dead) continue;
                                ob.Die();
                            }
                        }

                        return;
        }
        /// <summary>Handles @CHANGECLASS.</summary>
        internal void Cmd_CHANGECLASS(string[] parts)
        {
            CharacterInfo data;

                        if (!IsGM && !Settings.TestServer) return;

                        data = parts.Length <= 2 || !IsGM ? Info : Envir.GetCharacterInfo(parts[1]);

                        if (data == null) return;

                        MirClass mirClass;

                        if (!Enum.TryParse(parts[parts.Length - 1], true, out mirClass) || data.Class == mirClass) return;

                        data.Class = mirClass;

                        ReceiveChat(GameLanguage.Get("Player {0} has been changed to {1}", data.Name, data.Class), ChatType.System);
                        MessageQueue.Enqueue(GameLanguage.Get("Player {0} has been changed to {1} by {2}", data.Name, data.Class, Name));

                        Helpers.ChatSystem.SystemMessage(chatMessage: GameLanguage.Get("{0} class changed to {1} by GM: {2}", data.Player.Name, data.Class.ToString(), Name));

                        if (data.Player != null)
                            data.Player.Connection.LogOut();
                        return;
        }
        /// <summary>Handles @HAIR.</summary>
        internal void Cmd_HAIR(string[] parts)
        {

                        if (!IsGM && !Settings.TestServer) return;

                        if (parts.Length < 2)
                        {
                            Info.Hair = (byte)Envir.Random.Next(0, 9);
                        }
                        else
                        {
                            byte tempByte = 0;

                            byte.TryParse(parts[1], out tempByte);

                            Info.Hair = tempByte;
                        }
                        return;
        }
        /// <summary>Handles @DECO.</summary>
        internal void Cmd_DECO(string[] parts)
        {
            int tempInt;

                        if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;

                        int.TryParse(parts[1], out tempInt);

                        DecoObject decoOb = new DecoObject
                        {
                            Image = tempInt,
                            CurrentMap = CurrentMap,
                            CurrentLocation = CurrentLocation,
                        };

                        CurrentMap.AddObject(decoOb);
                        decoOb.Spawned();

                        Enqueue(decoOb.GetInfo());

                        return;
        }
        /// <summary>Handles @ADJUSTPKPOINT.</summary>
        internal void Cmd_ADJUSTPKPOINT(string[] parts)
        {
            int tempInt;
            PlayerObject player;

                        if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;

                        if (parts.Length > 2)
                        {
                            if (!IsGM) return;

                            player = Envir.GetPlayer(parts[1]);

                            if (player == null) return;


                            int.TryParse(parts[2], out tempInt);
                        }
                        else
                        {
                            player = this;
                            int.TryParse(parts[1], out tempInt);
                        }

                        player.PKPoints = tempInt;

                        return;
        }
        /// <summary>Handles @AWAKENING.</summary>
        internal void Cmd_AWAKENING(string[] parts)
        {
            Packet p;

                        {
                            if ((!IsGM && !Settings.TestServer) || parts.Length < 3) return;

                            ItemType type;

                            if (!Enum.TryParse(parts[1], true, out type)) return;

                            AwakeType awakeType;

                            if (!Enum.TryParse(parts[2], true, out awakeType)) return;

                            foreach (UserItem temp in Info.Equipment)
                            {
                                if (temp == null) continue;

                                ItemInfo realItem = Functions.GetRealItem(temp.Info, Info.Level, Info.Class, Envir.ItemInfoList);

                                if (realItem.Type == type)
                                {
                                    Awake awake = temp.Awake;
                                    bool[] isHit;
                                    int result = awake.UpgradeAwake(temp, awakeType, out isHit);
                                    switch (result)
                                    {
                                        case -1:
                                            ReceiveChat(GameLanguage.Get("{0} : Condition Error.", temp.FriendlyName), ChatType.System);
                                            break;
                                        case 0:
                                            ReceiveChat(GameLanguage.Get("{0} : Upgrade Failed.", temp.FriendlyName), ChatType.System);
                                            break;
                                        case 1:
                                            ReceiveChat(GameLanguage.Get("{0} : AWAKE Level {1}, value {2}~{3}.", temp.FriendlyName, awake.GetAwakeLevel(), awake.GetAwakeValue(), awake.GetAwakeValue()), ChatType.System);
                                            p = new S.RefreshItem { Item = temp };
                                            Enqueue(p);
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                        return;
        }
        /// <summary>Handles @REMOVEAWAKENING.</summary>
        internal void Cmd_REMOVEAWAKENING(string[] parts)
        {
            Packet p;

                        {
                            if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;

                            ItemType type;

                            if (!Enum.TryParse(parts[1], true, out type)) return;

                            foreach (UserItem temp in Info.Equipment)
                            {
                                if (temp == null) continue;

                                ItemInfo realItem = Functions.GetRealItem(temp.Info, Info.Level, Info.Class, Envir.ItemInfoList);

                                if (realItem.Type == type)
                                {
                                    Awake awake = temp.Awake;
                                    int result = awake.RemoveAwake();
                                    switch (result)
                                    {
                                        case 0:
                                            ReceiveChat(GameLanguage.Get("{0} : Remove failed Level 0", temp.FriendlyName), ChatType.System);
                                            break;
                                        case 1:
                                            ReceiveChat(GameLanguage.Get("{0} : Remove success. Level {1}", temp.FriendlyName, temp.Awake.GetAwakeLevel()), ChatType.System);
                                            p = new S.RefreshItem { Item = temp };
                                            Enqueue(p);
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                        return;
        }
        /// <summary>Handles @STARTWAR.</summary>
        internal void Cmd_STARTWAR(string[] parts)
        {

                        if (!IsGM) return;
                        if (parts.Length < 2) return;

                        GuildObject enemyGuild = Envir.GetGuild(parts[1]);

                        if (MyGuild == null)
                        {
                            ReceiveChat(GameLanguage.Get("You are not in a guild."), ChatType.System);
                        }

                        if (MyGuild.Ranks[0] != MyGuildRank)
                        {
                            ReceiveChat(GameLanguage.Get("You must be a leader to start a war."), ChatType.System);
                            return;
                        }

                        if (enemyGuild == null)
                        {
                            ReceiveChat(GameLanguage.Get("Could not find guild {0}.", parts[1]), ChatType.System);
                            return;
                        }

                        if (MyGuild == enemyGuild)
                        {
                            ReceiveChat(GameLanguage.Get("Cannot go to war with your own guild."), ChatType.System);
                            return;
                        }

                        if (enemyGuild.Name == Settings.NewbieGuild)
                        {
                            ReceiveChat(GameLanguage.Get("Cannot go to war with new players guild."), ChatType.System);
                            return;
                        }

                        if (MyGuild.WarringGuilds.Contains(enemyGuild))
                        {
                            ReceiveChat(GameLanguage.Get("Already at war with this guild."), ChatType.System);
                            return;
                        }

                        if (MyGuild.GoToWar(enemyGuild))
                        {
                            ReceiveChat(GameLanguage.Get("You started a war with {0}.", parts[1]), ChatType.System);
                            enemyGuild.SendMessage(GameLanguage.Get("{0} has started a war", MyGuild.Name), ChatType.System);
                        }
                        return;
        }
        /// <summary>Handles @INFO.</summary>
        internal void Cmd_INFO(string[] parts)
        {

                        {
                            if (!IsGM && !Settings.TestServer) return;

                            MapObject ob = null;

                            if (parts.Length < 2)
                            {
                                Point target = Functions.PointMove(CurrentLocation, Direction, 1);
                                if (!CurrentMap.ValidPoint(target)) return;
                                Cell cell = CurrentMap.GetCell(target);

                                if (cell.Objects == null || cell.Objects.Count < 1) return;

                                ob = cell.Objects[0];
                            }
                            else
                            {
                                ob = Envir.GetPlayer(parts[1]);
                            }

                            if (ob == null) return;

                            switch (ob.Race)
                            {
                                case ObjectType.Player:
                                    PlayerObject plOb = (PlayerObject)ob;
                                    ReceiveChat(GameLanguage.Get("--Player Info--"), ChatType.System2);
                                    ReceiveChat(GameLanguage.Get("Name : {0}, Level : {1}, X : {2}, Y : {3}", plOb.Name, plOb.Level, plOb.CurrentLocation.X, plOb.CurrentLocation.Y), ChatType.System2);
                                    break;
                                case ObjectType.Monster:
                                    MonsterObject monOb = (MonsterObject)ob;
                                    ReceiveChat(GameLanguage.Get("--Monster Info--"), ChatType.System2);
                                    ReceiveChat(GameLanguage.Get("ID : {0}, Name : {1}", monOb.Info.Index, monOb.Name), ChatType.System2);
                                    ReceiveChat(GameLanguage.Get("Level : {0}, X : {1}, Y : {2}, Dir: {3}", monOb.Level, monOb.CurrentLocation.X, monOb.CurrentLocation.Y, monOb.Direction), ChatType.System2);
                                    ReceiveChat(GameLanguage.Get("HP : {0}, MinDC : {1}, MaxDC : {2}", monOb.Info.Stats[Stat.HP], monOb.Stats[Stat.MinDC], monOb.Stats[Stat.MaxDC]), ChatType.System2);
                                    break;
                                case ObjectType.Merchant:
                                    NPCObject npcOb = (NPCObject)ob;
                                    ReceiveChat(GameLanguage.Get("--NPC Info--"), ChatType.System2);
                                    ReceiveChat(GameLanguage.Get("ID : {0}, Name : {1}", npcOb.Info.Index, npcOb.Name), ChatType.System2);
                                    ReceiveChat(GameLanguage.Get("X : {0}, Y : {1}", ob.CurrentLocation.X, ob.CurrentLocation.Y), ChatType.System2);
                                    ReceiveChat(GameLanguage.Get("File : {0}", npcOb.Info.FileName), ChatType.System2);
                                    break;
                            }
                        }
                        return;
        }
        /// <summary>Handles @CLEARQUESTS.</summary>
        internal void Cmd_CLEARQUESTS(string[] parts)
        {
            PlayerObject player;

                        if (!IsGM && !Settings.TestServer) return;

                        player = parts.Length > 1 && IsGM ? Envir.GetPlayer(parts[1]) : this;

                        if (player == null)
                        {
                            ReceiveChat(GameLanguage.Get("{0} is not online.", parts[1]), ChatType.System);
                            return;
                        }

                        for (int i = player.CurrentQuests.Count - 1; i >= 0; i--)
                        {
                            SendUpdateQuest(player.CurrentQuests[i], QuestState.Remove);
                        }

                        player.CompletedQuests.Clear();
                        player.GetCompletedQuests();

                        return;
        }
        /// <summary>Handles @SETQUEST.</summary>
        internal void Cmd_SETQUEST(string[] parts)
        {
            PlayerObject player;

                        if ((!IsGM && !Settings.TestServer) || parts.Length < 3) return;

                        player = parts.Length > 3 && IsGM ? Envir.GetPlayer(parts[3]) : this;

                        if (player == null)
                        {
                            ReceiveChat(GameLanguage.Get("{0} is not online.", parts[3]), ChatType.System);
                            return;
                        }

                        int.TryParse(parts[1], out int questID);
                        int.TryParse(parts[2], out int questState);

                        if (questID < 1) return;

                        var activeQuest = player.CurrentQuests.FirstOrDefault(e => e.Index == questID);

                        //remove from active list
                        if (activeQuest != null)
                        {
                            player.SendUpdateQuest(activeQuest, QuestState.Remove);
                        }

                        switch (questState)
                        {
                            case 0: //cancel
                                if (player.CompletedQuests.Contains(questID))
                                {
                                    player.CompletedQuests.Remove(questID);
                                }
                                break;
                            case 1: //complete
                                if (!player.CompletedQuests.Contains(questID))
                                {
                                    player.CompletedQuests.Add(questID);
                                }
                                break;
                        }

                        player.GetCompletedQuests();
                        return;
        }
        /// <summary>Handles @STARTCONQUEST.</summary>
        internal void Cmd_STARTCONQUEST(string[] parts)
        {

                        {
                            if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;
                            int conquestID;

                            if (parts.Length < 1)
                            {
                                ReceiveChat(GameLanguage.Get("The Syntax is /StartConquest [ConquestID]"), ChatType.System);
                                return;
                            }

                            if (MyGuild == null)
                            {
                                ReceiveChat(GameLanguage.Get("You need to be in a guild to start a War"), ChatType.System);
                                return;
                            }

                            else if (!int.TryParse(parts[1], out conquestID)) return;

                            ConquestObject tempConq = Envir.Conquests.FirstOrDefault(t => t.Info.Index == conquestID);

                            if (tempConq != null)
                            {
                                tempConq.StartType = ConquestType.Forced;
                                tempConq.WarIsOn = !tempConq.WarIsOn;
                                tempConq.GuildInfo.AttackerID = MyGuild.Guildindex;
                            }
                            else return;
                            ReceiveChat(GameLanguage.Get("{0} War Started.", tempConq.Info.Name), ChatType.System);
                            MessageQueue.Enqueue(GameLanguage.Get("{0} War Started.", tempConq.Info.Name));

                            foreach (var pl in Envir.Players)
                            {
                                if (tempConq.WarIsOn)
                                {
                                    pl.ReceiveChat(GameLanguage.Get("{0} War Started.", tempConq.Info.Name), ChatType.System);
                                }
                                else
                                {
                                    pl.ReceiveChat(GameLanguage.Get("{0} War Stopped.", tempConq.Info.Name), ChatType.System);
                                }

                                pl.BroadcastInfo();
                            }
                        }
                        return;
        }
        /// <summary>Handles @RESETCONQUEST.</summary>
        internal void Cmd_RESETCONQUEST(string[] parts)
        {

                        {
                            if ((!IsGM && !Settings.TestServer) || parts.Length < 2) return;
                            int conquestID;

                            if (parts.Length < 1)
                            {
                                ReceiveChat(GameLanguage.Get("The Syntax is /ResetConquest [ConquestID]"), ChatType.System);
                                return;
                            }

                            if (MyGuild == null)
                            {
                                ReceiveChat(GameLanguage.Get("You need to be in a guild to start a War"), ChatType.System);
                                return;
                            }

                            else if (!int.TryParse(parts[1], out conquestID)) return;

                            ConquestObject resetConq = Envir.Conquests.FirstOrDefault(t => t.Info.Index == conquestID);

                            if (resetConq != null && !resetConq.WarIsOn)
                            {
                                resetConq.Reset();
                                ReceiveChat(GameLanguage.Get("{0} has been reset.", resetConq.Info.Name), ChatType.System);
                            }
                            else
                            {
                                ReceiveChat(GameLanguage.Get("Conquest not found or War is currently on."), ChatType.System);
                            }
                        }
                        return;
        }
        /// <summary>Handles @REVIVE.</summary>
        internal void Cmd_REVIVE(string[] parts)
        {
            PlayerObject player;

                        if (!IsGM) return;

                        if (parts.Length < 2)
                        {
                            RefreshStats();
                            SetHP(Stats[Stat.HP]);
                            SetMP(Stats[Stat.MP]);
                            Revive(MaxHealth, true);
                        }
                        else
                        {
                            player = Envir.GetPlayer(parts[1]);
                            if (player == null) return;

                            player.Revive(MaxHealth, true);

                            Helpers.ChatSystem.SystemMessage(chatMessage: GameLanguage.Get("{0} was revived to full health by GM: {1}", player.Name, Name));
                        }
                        return;
        }
        /// <summary>Handles @DELETESKILL.</summary>
        internal void Cmd_DELETESKILL(string[] parts)
        {
            PlayerObject player;

                        if ((!IsGM) || parts.Length < 2) return;
                        Spell skill1;

                        if (!Enum.TryParse(parts.Length > 2 ? parts[2] : parts[1], true, out skill1)) return;

                        if (skill1 == Spell.None) return;

                        if (parts.Length > 2)
                        {
                            if (!IsGM) return;
                            player = Envir.GetPlayer(parts[1]);

                            if (player == null)
                            {
                                ReceiveChat(GameLanguage.Get("Player {0} was not found", parts[1]), ChatType.System);
                                return;
                            }
                        }
                        else
                        {
                            player = this;
                        }

                        if (player == null) return;

                        var magics = new UserMagic(skill1);
                        bool removed = false;

                        for (var i = player.Info.Magics.Count - 1; i >= 0; i--)
                        {
                            if (player.Info.Magics[i].Spell != skill1) continue;

                            player.Info.Magics.RemoveAt(i);
                            player.Enqueue(new S.RemoveMagic { PlaceId = i });
                            removed = true;
                        }

                        if (removed)
                        {
                            ReceiveChat(GameLanguage.Get("You have deleted skill {0} from player {1}", skill1.ToString(), player.Name), ChatType.Hint);
                            player.ReceiveChat(GameLanguage.Get("{0} has been removed from you.", skill1), ChatType.Hint);

                            Helpers.ChatSystem.SystemMessage(chatMessage: GameLanguage.Get("{0} Skill {1} was removed by GM: {2}", player, skill1.ToString(), Name));
                        }
                        else
                        {
                            ReceiveChat(GameLanguage.Get("Unable to delete skill, skill not found"), ChatType.Hint);
                        }

                        return;
        }
        /// <summary>Handles @SETLIGHT.</summary>
        internal void Cmd_SETLIGHT(string[] parts)
        {

                        if ((!IsGM) || parts.Length < 2) return;

                        if (!byte.TryParse(parts[1], out byte light)) return;

                        Light = light;

                        Enqueue(GetUpdateInfo());
                        Broadcast(GetUpdateInfo());
                        return;
        }
    }
}
