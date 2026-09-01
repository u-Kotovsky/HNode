using System.Collections.Generic;
using UnityEngine;

public class Fixture
{
    public string Name;
    public string Description;
    
    public List<FixtureParameter> Parameters { get; set; } = new List<FixtureParameter>();

    public Fixture()
    {
        foreach (var fixtureParameter in Parameters)
        {
            fixtureParameter.OnRemove += () =>
            {
                Debug.Log("Remove parameter from list");
                Parameters.Remove(fixtureParameter);
            };
        }
    }

    public void EditMode(bool edit)
    {
        // todo: add/remove parameters
        foreach (var fixtureParameter in Parameters)
        {
            fixtureParameter.EditMode(edit);
        }
    }
}
