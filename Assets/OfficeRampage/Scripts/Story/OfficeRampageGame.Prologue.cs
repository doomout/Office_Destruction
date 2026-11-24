using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private float prologueAge;
        private const float PrologueDuration=36;
        private void TickPrologue(float dt) { prologueAge=Mathf.Min(PrologueDuration,prologueAge+dt); }
        private void BeginStoryDestruction()
        {
            endlessMode=false; ResetRun();
            StoryLine("좋아. 승인권자 나오라고 해.",5);
        }
        private bool DrawPrologue()
        {
            if(prologueAge>=PrologueDuration)return false;
            int beat=prologueAge<9?0:prologueAge<26?1:2;
            Box(new Rect(100,212,1080,370),new Color(.025f,.045f,.075f));
            string caption;
            if(beat==0)
            {
                GUI.Label(new Rect(130,225,900,42),"금요일 23:48 · 아직 사무실",menuHeading);
                // Empty desks and the only illuminated workstation make the overtime readable without voiceover.
                for(int i=0;i<5;i++)
                {
                    float x=160+i*190; bool active=i==2;
                    Box(new Rect(x,365,130,9),new Color(.22f,.26f,.3f));
                    Box(new Rect(x+8,374,8,65),Color.gray);Box(new Rect(x+115,374,8,65),Color.gray);
                    Box(new Rect(x+30,305,70,47),active?Cyan:new Color(.07f,.1f,.14f));
                    Box(new Rect(x+60,350,9,15),Color.gray);
                    if(active){Box(new Rect(x+45,380,44,65),Orange);GUI.Label(new Rect(x-10,455,190,35),"나만 아직 퇴근 전",menuSmall);}
                }
                caption="어제도 야근했다. 모두 떠난 사무실에 나만 남았다.";
            }
            else if(beat==1)
            {
                GUI.Label(new Rect(130,225,900,42),"토요일 07:12 · 상사에게 온 메시지",menuHeading);
                string[] messages={"오늘 잠깐만 나와줄 수 있지?","주말 근무 신청은 하지 말고. 승인하기 번거로워.","그리고 어제 올린 보고서, 처음부터 다시 하자."};
                int count=Mathf.Clamp(1+(int)((prologueAge-9)/5),1,3);
                for(int i=0;i<count;i++)
                {
                    Box(new Rect(145,290+i*80,990,65),new Color(.1f,.18f,.24f));
                    GUI.Label(new Rect(170,303+i*80,935,45),messages[i],menuText);
                }
                caption="주말까지 반납하라면서, 일한 기록도 남기지 말란다.";
            }
            else
            {
                GUI.Label(new Rect(130,225,900,42),"토요일 08:59 · 회사 출입구",menuHeading);
                Box(new Rect(160,295,240,205),Navy);Box(new Rect(175,310,210,45),Cyan);
                GUI.Label(new Rect(190,365,200,50),"사원증 인식",menuText);
                GUI.Label(new Rect(450,310,660,150),"출근이 확인되었습니다.\n퇴근 권한은 없습니다.",menuHeading);
                Box(new Rect(450,485,640,5),Orange);
                caption="도저히 참을 수 없다. 승인권자가 누구야?";
            }
            GUI.Label(new Rect(130,590,1030,42),caption,menuText);
            Box(new Rect(100,645,850,4),Navy);Box(new Rect(100,645,850*prologueAge/PrologueDuration,4),Cyan);
            if(MenuButton(new Rect(970,630,210,38),"건너뛰기"))prologueAge=PrologueDuration;
            return true;
        }
    }
}
