using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillPanel : MonoBehaviour
{
    List<Skill> skills = new List<Skill>();
    [SerializeField]
    GameObject buttonPrefab;

    void Start() {
        SpawnSkillButtons();
        NetworkSession.singleton.skillPanel = gameObject;
        gameObject.SetActive(false);
    }

    void SpawnSkillButtons() {
        skills = CustomRoomPlayer.localPlayerRoom.skills;

        foreach (Skill skill in skills) {
            GameObject newButton = Instantiate(buttonPrefab) as GameObject;
            newButton.transform.SetParent(transform, false);
            newButton.name = skill.name;
            newButton.GetComponentInChildren<Text>().text = skill.name;
            newButton.GetComponentInChildren<Button>().onClick.AddListener(() => gameObject.SetActive(false));
            newButton.GetComponentInChildren<Button>().onClick.AddListener(() => Player.localPlayer.GetComponent<PlayerAttack>().UseSkill(skill));
        }
    }
}
