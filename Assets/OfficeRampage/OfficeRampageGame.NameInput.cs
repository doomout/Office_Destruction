using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private OfficeNameEditor nameEditor;
        private int editingRank,editingPerson;
        private void OpenNameEditor(int rank,int person)
        {
            if(nameEditor==null) nameEditor=gameObject.AddComponent<OfficeNameEditor>();
            editingRank=rank; editingPerson=person;
            nameEditor.Open(companyDraft.ranks[rank].people[person].name);
        }
        private bool ApplyEditedName()
        {
            if(nameEditor==null || !nameEditor.Active || nameEditor.Composition.Length>0 || string.IsNullOrWhiteSpace(nameEditor.Text)) return false;
            companyDraft.ranks[editingRank].people[editingPerson].name=nameEditor.Text.Trim(); nameEditor.Close(); return true;
        }
        private void DrawNameEditor()
        {
            if(nameEditor==null || !nameEditor.Active) return;
            Box(new Rect(0,0,1280,720),new Color(0,0,0,.8f));
            Box(new Rect(200,230,880,310),Navy);
            GUI.Label(new Rect(230,250,820,42),OfficeCompanySettings.Ranks[editingRank]+" "+(editingPerson+1)+"번 · 이름 편집",menuHeading);
            Box(new Rect(230,315,820,54),new Color(.15f,.24f,.3f));
            GUI.Label(new Rect(245,323,790,40),nameEditor.DisplayText,menuText);
            GUI.Label(new Rect(230,385,820,65),"한/영 키로 전환 · 한글 조합을 마친 뒤 적용 · 최대 24자\nCtrl+A 전체 선택 · Ctrl+V 붙여넣기 · 방향키로 커서 이동",menuSmall);
            if(Event.current.type==EventType.Repaint)
            {
                float x=245+Mathf.Min(760,menuText.CalcSize(new GUIContent(nameEditor.Text.Substring(0,nameEditor.Caret))).x);
                Vector3 point=GUI.matrix.MultiplyPoint3x4(new Vector3(x,370,0));
                nameEditor.PositionCandidateWindow(new Vector2(point.x,Screen.height-point.y));
            }
            if(MenuButton(new Rect(490,465,240,48),"취소")) nameEditor.Close();
            bool before=GUI.enabled; GUI.enabled=before && nameEditor.Composition.Length==0 && !string.IsNullOrWhiteSpace(nameEditor.Text);
            if(MenuButton(new Rect(760,465,260,48),"적용",true)) ApplyEditedName();
            GUI.enabled=before;
        }
    }
}
