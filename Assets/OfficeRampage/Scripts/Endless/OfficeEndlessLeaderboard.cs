using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace OfficeRampage
{
    [Serializable]
    public sealed class OfficeEndlessRecord
    {
        public string runId,playerName;
        public int seconds,kills,score;
        public long utcTicks;
        public long Total=>score+(long)seconds*10+(long)kills*100;
    }
    [Serializable]
    public sealed class OfficeEndlessLeaderboard
    {
        public const string SaveKey="OfficeRampage.Endless.Leaderboard.v1";
        public const string NameKey="OfficeRampage.Endless.LastPlayerName";
        public List<OfficeEndlessRecord> records=new List<OfficeEndlessRecord>();
        public static string CleanName(string value)
        {
            var text=new StringBuilder();foreach(char c in value??"")if(!char.IsControl(c))text.Append(c);
            string name=text.ToString().Trim();if(name.Length>24)name=name.Substring(0,24);
            if(name.Length>0&&char.IsHighSurrogate(name[name.Length-1]))name=name.Substring(0,name.Length-1);
            return name;
        }
        public static int Compare(OfficeEndlessRecord a,OfficeEndlessRecord b)
        {
            int order=b.Total.CompareTo(a.Total);if(order!=0)return order;
            order=b.seconds.CompareTo(a.seconds);if(order!=0)return order;
            order=b.kills.CompareTo(a.kills);if(order!=0)return order;
            order=b.score.CompareTo(a.score);if(order!=0)return order;
            order=a.utcTicks.CompareTo(b.utcTicks);return order!=0?order:string.CompareOrdinal(a.runId,b.runId);
        }
        public void Normalize()
        {
            if(records==null)records=new List<OfficeEndlessRecord>();var ids=new HashSet<string>();
            records.RemoveAll(r=>r==null||string.IsNullOrEmpty(r.runId)||string.IsNullOrEmpty(CleanName(r.playerName))||r.seconds<0||r.kills<0||r.score<0||!ids.Add(r.runId));
            foreach(var record in records)record.playerName=CleanName(record.playerName);
            records.Sort(Compare);if(records.Count>10)records.RemoveRange(10,records.Count-10);
        }
        public static OfficeEndlessLeaderboard Load()
        {
            OfficeEndlessLeaderboard result=null;
            try{result=JsonUtility.FromJson<OfficeEndlessLeaderboard>(PlayerPrefs.GetString(SaveKey,""));}catch(ArgumentException){}
            if(result==null)result=new OfficeEndlessLeaderboard();result.Normalize();return result;
        }
        public int RankOf(string id)=>records.FindIndex(r=>r.runId==id)+1;
        public int Add(OfficeEndlessRecord record)
        {
            if(RankOf(record.runId)==0)records.Add(record);Normalize();return RankOf(record.runId);
        }
        public void Save(){Normalize();PlayerPrefs.SetString(SaveKey,JsonUtility.ToJson(this));PlayerPrefs.Save();}
    }
}
