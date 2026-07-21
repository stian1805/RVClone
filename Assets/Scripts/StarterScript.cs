using UnityEngine;

public class StarterScript : MonoBehaviour
{
    [SerializeField] int myNumber;
    [SerializeField] string myName;
    [SerializeField] bool myChoice;
    [SerializeField] GameObject myGate;

    private void Start()
    {
        myNumber = 4;
        myName = "Gateson";
        myChoice = true;
        myGate.SetActive(true);
    }

    private void Update()
    {
        // Called once per frame.
    }

}
