using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;

namespace FinalExamSystem
{
    public class Player
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastLogin { get; set; }
        public int VipLevel { get; set; }
        public int Level { get; set; }
        public int Gold { get; set; }
    }

    public class AwardedVipPlayer
    {
        public string Name { get; set; }
        public int VipLevel { get; set; }
        public int Level { get; set; }
        public int CurrentGold { get; set; }
        public int AwardedGoldAmount { get; set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            bool running = true;

            while (running)
            {
                Console.Clear();
                Console.WriteLine(" FINAL EXAM PLAYER SYSTEM");
                Console.WriteLine("1. Exit");
                Console.WriteLine("2. View Raw Player Data");
                Console.WriteLine("3. Bài 1 - Inactive/Low-Level Analysis");
                Console.WriteLine("4. Bài 1.2 - List & Upload Level ≤ 10 Players");
                Console.WriteLine("5. Bài 2 - VIP Rewards and Upload");
                Console.Write("👉 Choose (1-5): ");
                string input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        running = false;
                        break;
                    case "2":
                        await ViewRawPlayerData();
                        break;
                    case "3":
                        await AnalyzeAndUploadLowLevelInactivePlayers();
                        break;
                    case "4":
                        await ListAndUploadLevelTenOrLower();
                        break;
                    case "5":
                        await AnalyzeAndUploadVipAwardPlayers();
                        break;
                    default:
                        Console.WriteLine(" Invalid input. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        static async Task<List<Player>> FetchPlayers()
        {
            string url = "https://raw.githubusercontent.com/NTH-VTC/OnlineDemoC-/refs/heads/main/lab12_players.json";
            var client = new HttpClient();
            try
            {
                var json = await client.GetStringAsync(url);
                return JsonConvert.DeserializeObject<List<Player>>(json);
            }
            finally
            {
                client.Dispose();
            }
        }

        static async Task ViewRawPlayerData()
        {
            var players = await FetchPlayers();
            Console.WriteLine("\n RAW PLAYER DATA");
            foreach (var p in players)
                Console.WriteLine($"{p.Name} | Active: {p.IsActive} | LastLogin: {p.LastLogin} | VIP: {p.VipLevel} | Level: {p.Level} | Gold: {p.Gold}");
            Console.WriteLine("\nPress Enter to continue...");
            Console.ReadLine();
        }

        static async Task AnalyzeAndUploadLowLevelInactivePlayers()
        {
            var now = new DateTime(2025, 06, 30, 0, 0, 0, DateTimeKind.Utc);
            var players = await FetchPlayers();

            var final_exam_bai1_low_level_player = players
                .Where(p => !p.IsActive || (now - p.LastLogin).TotalDays > 5)
                .ToList();

            Console.WriteLine("\n🔻 BÀI 1: LOW-LEVEL INACTIVE PLAYERS");
            foreach (var p in final_exam_bai1_low_level_player)
                Console.WriteLine($"{p.Name} | Active: {p.IsActive} | LastLogin: {p.LastLogin}");

            var firebase = new FirebaseClient("https://lab-12-final-85ae7-default-rtdb.asia-southeast1.firebasedatabase.app/");
            await firebase
                .Child("final_exam_bai1_low_level_player")
                .PutAsync(final_exam_bai1_low_level_player);

            Console.WriteLine("\n Uploaded to Firebase. Press Enter to continue...");
            Console.ReadLine();
        }

        static async Task ListAndUploadLevelTenOrLower()
        {
            var players = await FetchPlayers();

            var levelTenPlayers = players
                .Where(p => p.Level <= 10)
                .OrderBy(p => p.Level)
                .ToList();

            Console.WriteLine("\n📉 BÀI 1.2: NGƯỜI CHƠI CẤP THẤP");
            Console.WriteLine("Tên Người Chơi   | Level | Gold Hiện Tại");
            Console.WriteLine("-----------------|-------|--------------");
            foreach (var p in levelTenPlayers)
                Console.WriteLine($"{p.Name,-17}| {p.Level,-6}| {p.Gold}");
            Console.WriteLine("-----------------|-------|--------------");

            var firebase = new FirebaseClient("https://lab-12-final-85ae7-default-rtdb.asia-southeast1.firebasedatabase.app/");
            var node = firebase.Child("final_exam_bai1_2_low_level_list");
            for (int i = 0; i < levelTenPlayers.Count; i++)
                await node.Child((i + 1).ToString()).PutAsync(levelTenPlayers[i]);

            Console.WriteLine("\n✅ Level ≤ 10 list uploaded to Firebase. Press Enter to continue...");
            Console.ReadLine();
        }

        static async Task AnalyzeAndUploadVipAwardPlayers()
        {
            var players = await FetchPlayers();

            var top3 = players
                .Where(p => p.VipLevel > 0)
                .OrderByDescending(p => p.Level)
                .Take(3)
                .ToList();

            var awards = new[] { 2000, 1500, 1000 };
            var final_exam_bai2_top3_vip_awards = new List<AwardedVipPlayer>();

            for (int i = 0; i < top3.Count; i++)
            {
                var p = top3[i];
                final_exam_bai2_top3_vip_awards.Add(new AwardedVipPlayer
                {
                    Name = p.Name,
                    VipLevel = p.VipLevel,
                    Level = p.Level,
                    CurrentGold = p.Gold,
                    AwardedGoldAmount = awards[i]
                });
            }

            Console.WriteLine("\n🏆 BÀI 2: TOP 3 VIP REWARDS");
            foreach (var a in final_exam_bai2_top3_vip_awards)
                Console.WriteLine($"{a.Name} | VIP: {a.VipLevel} | Level: {a.Level} | Gold: {a.CurrentGold} ➡ Awarded: {a.AwardedGoldAmount}");

            var firebase = new FirebaseClient("https://lab-12-final-85ae7-default-rtdb.asia-southeast1.firebasedatabase.app/");
            await firebase
                .Child("final_exam_bai2_top3_vip_awards")
                .PutAsync(final_exam_bai2_top3_vip_awards);

            Console.WriteLine("\n VIP Rewards uploaded to Firebase. Press Enter to continue...");
            Console.ReadLine();
        }
    }
}