using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car
{
    public CarColor color;
    public CarType carType;
}
public enum CarType
{
    family = 1,
    coupe = 2,
    sedan = 3,
    pickup = 4,
    sport = 5
}
public enum CarColor
{
    black = 0,
    blue = 1,
    green = 2,
    red = 3,
    yellow = 4
}