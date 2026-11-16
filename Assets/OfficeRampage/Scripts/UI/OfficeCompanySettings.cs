using System;
using UnityEngine;
namespace OfficeRampage
{
    [Serializable]
    public sealed class OfficeCompanySettings
    {
        public const string SaveKey="OfficeRampage.Company.v2", LegacySaveKey="OfficeRampage.Company.v1";
        public const int RepresentativeRank=10;
        public static readonly string[] Ranks={"사원","주임","대리","과장","차장","부장","이사","상무","전무","부사장","대표"};
        public static readonly int[] Counts={10,8,6,5,4,3,2,2,1,1,1};
        public static readonly int[] Rooms={1,2,2,2,2,2,3,3,3,3,4};
        public static readonly int[] Health={20,40,60,80,100,120,140,160,180,220,500};
        // Guards stay implemented, but disabled in the current roster.
        public const int GuardCount=0, DogCount=5;
        private static readonly string[] Surnames={"김","이","박","최","정","강","조","윤","장","임","한","오","서","신","권","황","안","송","류","홍"};
        private static readonly string[] GivenNames={"민준","서연","지우","도윤","하윤","예준","수빈","지호","서준","지민","하린","유진","시우","다은","현우","채원","서진","지원","예린","수현","민서","도현","유나","태윤","시온","소율","준서","연우","은서","지안"};
        [Serializable] public sealed class Person { public string name=""; public bool female; }
        [Serializable] public sealed class RankSetting { public Person[] people; }
        public RankSetting[] ranks;
        [Serializable] private sealed class LegacyRow { public string name=""; public int count; public bool female; }
        [Serializable] private sealed class LegacySettings { public LegacyRow[] ranks; }
        private static Person RandomPerson()
        { return new Person { name=Surnames[UnityEngine.Random.Range(0,Surnames.Length)]+GivenNames[UnityEngine.Random.Range(0,GivenNames.Length)],female=UnityEngine.Random.value<.5f }; }
        public static OfficeCompanySettings CreateDefault()
        { var result=new OfficeCompanySettings(); result.Normalize(); return result; }
        public void Normalize()
        {
            var old=ranks; ranks=new RankSetting[Ranks.Length];
            for(int rank=0;rank<Ranks.Length;rank++)
            {
                var source=old!=null && rank<old.Length && old[rank]!=null?old[rank].people:null;
                ranks[rank]=new RankSetting { people=new Person[Counts[rank]] };
                for(int i=0;i<Counts[rank];i++)
                {
                    var person=source!=null && i<source.Length?source[i]:null;
                    if(person==null) person=RandomPerson();
                    string name=(person.name??"").Replace("\r","").Replace("\n","").Replace("\t"," ").Trim();
                    if(name.Length==0) name=RandomPerson().name;
                    ranks[rank].people[i]=new Person { name=name.Length>24?name.Substring(0,24):name,female=person.female };
                }
            }
        }
        public OfficeCompanySettings Copy()
        { var result=JsonUtility.FromJson<OfficeCompanySettings>(JsonUtility.ToJson(this)); result.Normalize(); return result; }
        public static OfficeCompanySettings Load()
        {
            try {
                var saved=JsonUtility.FromJson<OfficeCompanySettings>(PlayerPrefs.GetString(SaveKey,""));
                if(saved!=null) { saved.Normalize(); return saved; }
            } catch(ArgumentException) { }
            var result=CreateDefault();
            try {
                var old=JsonUtility.FromJson<LegacySettings>(PlayerPrefs.GetString(LegacySaveKey,""));
                if(old!=null && old.ranks!=null)
                    for(int rank=0;rank<Ranks.Length;rank++)
                    {
                        if(rank==9) continue;
                        int oldIndex=rank==RepresentativeRank?9:rank;
                        if(oldIndex>=old.ranks.Length || old.ranks[oldIndex]==null) continue;
                        var row=old.ranks[oldIndex];
                        // Preserve the old shared name in the first slot; never import old counts.
                        if(!string.IsNullOrWhiteSpace(row.name)) result.ranks[rank].people[0]=new Person { name=row.name,female=row.female };
                    }
            } catch(ArgumentException) { }
            result.Normalize(); return result;
        }
        public void Save() { Normalize(); PlayerPrefs.SetString(SaveKey,JsonUtility.ToJson(this)); PlayerPrefs.Save(); }
        public string UnitName(int rank,int index)
        { var person=ranks[rank].people[index]; return Ranks[rank]+" · "+person.name+(person.female?" (여)":" (남)"); }
        public int RoomCount(int room)
        {
            if(room==4) return DogCount;
            int total=room==1?GuardCount:0;
            for(int rank=0;rank<Ranks.Length;rank++) if(Rooms[rank]==room) total+=Counts[rank];
            return total;
        }
    }
}
