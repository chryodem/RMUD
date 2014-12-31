﻿public class solar : RMUD.Room
{
	public override void Initialize()
	{
        RoomType = RMUD.RoomType.Exterior;
        Short = "Palantine Villa - Solar";

        Move(GetObject("palantine/soranus"), this);

        OpenLink(RMUD.Direction.WEST, "palantine\\antechamber");
        OpenLink(RMUD.Direction.EAST, "palantine\\platos_closet");
	}
}