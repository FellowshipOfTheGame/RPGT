using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnSkillButtons : MonoBehaviour
{
    List<Skill> skills = new List<Skill>();
    [SerializeField]
    GameObject buttonPrefab;
    // Start is called before the first frame update
    void Start()
    {
        PlayerAttack playerAttack = Player.localPlayer.GetComponent<PlayerAttack>();
        skills = playerAttack.skills;

        foreach (Skill skill in skills) {
            GameObject newButton = Instantiate(buttonPrefab) as GameObject;
            newButton.transform.SetParent(transform, false);
            newButton.name = skill.name;
            newButton.GetComponentInChildren<Text>().text = skill.name;
            newButton.GetComponentInChildren<Button>().onClick.AddListener(() => gameObject.SetActive(false));
            newButton.GetComponentInChildren<Button>().onClick.AddListener(() => playerAttack.UseSkill(skill));
        }
    }
}
