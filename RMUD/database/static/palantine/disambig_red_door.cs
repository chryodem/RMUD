﻿public class disambig_red_door : RMUD.LockedDoor
{
    public override void Initialize()
    {
        Nouns.Add("RED");
        Locked = true;
        IsMatchingKey = k => object.ReferenceEquals(k, GetObject("palantine\\disambig_key"));
        Short = "red door";
    }
}
