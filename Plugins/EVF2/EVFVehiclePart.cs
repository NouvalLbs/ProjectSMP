using ProjectSMP.Plugins.ColAndreas;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.EVF2
{
    public enum VehiclePart
    {
        FrontTire = 1,
        RearTire = 2,
        RightFrontTire = 3,
        LeftFrontTire = 4,
        RightRearTire = 5,
        LeftRearTire = 6,
        RightMidTire = 7,
        LeftMidTire = 8,
        RightFrontSeat = 9,
        LeftFrontSeat = 10,
        RightRearSeat = 11,
        LeftRearSeat = 12,
        Hood = 13,
        Trunk = 14,
        PetrolCap = 15
    }

    public enum PetrolCapSide { None = 0, Right = 1, Left = 2, Back = 3, Front = 4 }

    public static class EVFVehiclePart
    {
        public static Vector3 GetPosNearVehiclePart(int vehicleId, VehiclePart part, float offset = 0.5f)
        {
            var v = BaseVehicle.Find(vehicleId);
            if (v == null) return Vector3.Zero;

            var base3 = v.Position;
            float a = v.Angle;
            var model = v.Model;

            float x = base3.X, y = base3.Y, z = base3.Z;

            switch (part)
            {
                case VehiclePart.FrontTire:
                    {
                        var info = BaseVehicle.GetModelInfo(model, VehicleModelInfoType.WheelsFront);
                        x += (info.X + offset) * Sin(-a) + info.Y * Sin(-a);
                        y += (info.X + offset) * Cos(-a) + info.Y * Cos(-a);
                        break;
                    }
                case VehiclePart.RearTire:
                    {
                        var info = BaseVehicle.GetModelInfo(model, VehicleModelInfoType.WheelsRear);
                        x += (info.X + offset) * Sin(-a + 180f) + info.Y * Sin(-a);
                        y += (info.X + offset) * Cos(-a + 180f) + info.Y * Cos(-a);
                        break;
                    }
                case VehiclePart.RightFrontTire:
                    {
                        var info = BaseVehicle.GetModelInfo(model, VehicleModelInfoType.WheelsFront);
                        x += (info.X + offset) * Sin(-a + 90f) + info.Y * Sin(-a);
                        y += (info.X + offset) * Cos(-a + 90f) + info.Y * Cos(-a);
                        break;
                    }
                case VehiclePart.LeftFrontTire:
                    {
                        var info = BaseVehicle.GetModelInfo(model, VehicleModelInfoType.WheelsFront);
                        x += (info.X + offset) * Sin(-a - 90f) + info.Y * Sin(-a);
                        y += (info.X + offset) * Cos(-a - 90f) + info.Y * Cos(-a);
                        break;
                    }
                case VehiclePart.RightRearTire:
                    {
                        var info = BaseVehicle.GetModelInfo(model, VehicleModelInfoType.WheelsRear);
                        x += (info.X + offset) * Sin(-a + 90f) + info.Y * Sin(-a);
                        y += (info.X + offset) * Cos(-a + 90f) + info.Y * Cos(-a);
                        break;
                    }
                case VehiclePart.LeftRearTire:
                    {
                        var info = BaseVehicle.GetModelInfo(model, VehicleModelInfoType.WheelsRear);
                        x += (info.X + offset) * Sin(-a - 90f) + info.Y * Sin(-a);
                        y += (info.X + offset) * Cos(-a - 90f) + info.Y * Cos(-a);
                        break;
                    }
                case VehiclePart.RightMidTire:
                    {
                        var info = BaseVehicle.GetModelInfo(model, VehicleModelInfoType.WheelsMiddle);
                        x += (info.X + offset) * Sin(-a + 90f) + info.Y * Sin(-a);
                        y += (info.X + offset) * Cos(-a + 90f) + info.Y * Cos(-a);
                        break;
                    }
                case VehiclePart.LeftMidTire:
                    {
                        var info = BaseVehicle.GetModelInfo(model, VehicleModelInfoType.WheelsMiddle);
                        x += (info.X + offset) * Sin(-a - 90f) + info.Y * Sin(-a);
                        y += (info.X + offset) * Cos(-a - 90f) + info.Y * Cos(-a);
                        break;
                    }
                case VehiclePart.RightFrontSeat:
                    {
                        var info = BaseVehicle.GetModelInfo(model, VehicleModelInfoType.FrontSeat);
                        x += (info.X + offset) * Sin(-a + 90f) + info.Y * Sin(-a);
                        y += (info.X + offset) * Cos(-a + 90f) + info.Y * Cos(-a);
                        break;
                    }
                case VehiclePart.LeftFrontSeat:
                    {
                        var info = BaseVehicle.GetModelInfo(model, VehicleModelInfoType.FrontSeat);
                        x += (info.X + offset) * Sin(-a - 90f) + info.Y * Sin(-a);
                        y += (info.X + offset) * Cos(-a - 90f) + info.Y * Cos(-a);
                        break;
                    }
                case VehiclePart.RightRearSeat:
                    {
                        var info = BaseVehicle.GetModelInfo(model, VehicleModelInfoType.RearSeat);
                        x += (info.X + offset) * Sin(-a + 90f) + info.Y * Sin(-a);
                        y += (info.X + offset) * Cos(-a + 90f) + info.Y * Cos(-a);
                        break;
                    }
                case VehiclePart.LeftRearSeat:
                    {
                        var info = BaseVehicle.GetModelInfo(model, VehicleModelInfoType.RearSeat);
                        x += (info.X + offset) * Sin(-a - 90f) + info.Y * Sin(-a);
                        y += (info.X + offset) * Cos(-a - 90f) + info.Y * Cos(-a);
                        break;
                    }
                case VehiclePart.Hood:
                    {
                        var size = BaseVehicle.GetModelInfo(model, VehicleModelInfoType.Size);
                        x += (size.Y / 2f + offset) * Sin(-a);
                        y += (size.Y / 2f + offset) * Cos(-a);
                        break;
                    }
                case VehiclePart.Trunk:
                    {
                        var size = BaseVehicle.GetModelInfo(model, VehicleModelInfoType.Size);
                        x += (size.Y / 2f + offset) * Sin(-a + 180f);
                        y += (size.Y / 2f + offset) * Cos(-a + 180f);
                        break;
                    }
                case VehiclePart.PetrolCap:
                    {
                        var info = BaseVehicle.GetModelInfo(model, VehicleModelInfoType.PetrolCap);
                        float ix = info.X, iy = info.Y;
                        var cap = GetPetrolCapSide(vehicleId);
                        if (cap == PetrolCapSide.None) iy -= 2f;
                        else if (cap == PetrolCapSide.Right) ix += offset;
                        else if (cap == PetrolCapSide.Left) ix -= offset;
                        else if (cap == PetrolCapSide.Back) iy -= offset;
                        else if (cap == PetrolCapSide.Front) iy += offset + 0.5f;
                        x = Sin(360f - a) * iy + Cos(360f - a) * ix + x;
                        y = Cos(360f - a) * iy - Sin(360f - a) * ix + y;
                        z += info.Z;
                        break;
                    }
            }

            // ColAndreas ground adjustment
            if (ColAndreasService.RayCastLine(x, y, z, x, y, z + 5f, out _, out _, out float gz) != 0 && gz != 0f)
                z = gz + offset;

            return new Vector3(x, y, z);
        }

        public static Vector3 GetVehiclePartLocalPos(int vehicleId, VehiclePart part)
        {
            var v = BaseVehicle.Find(vehicleId);
            if (v == null) return Vector3.Zero;

            var vpos = v.Position;
            float a = v.Angle;
            var model = v.Model;

            switch (part)
            {
                case VehiclePart.FrontTire:
                    return BaseVehicle.GetModelInfo(model, VehicleModelInfoType.WheelsFront);
                case VehiclePart.RearTire:
                    return BaseVehicle.GetModelInfo(model, VehicleModelInfoType.WheelsRear);
                case VehiclePart.RightFrontTire:
                case VehiclePart.LeftFrontTire:
                    return WorldToLocal(vpos, a, ApplyLateralOffset(vpos, a, BaseVehicle.GetModelInfo(model, VehicleModelInfoType.WheelsFront), part == VehiclePart.RightFrontTire ? 90f : -90f));
                case VehiclePart.RightRearTire:
                case VehiclePart.LeftRearTire:
                    return WorldToLocal(vpos, a, ApplyLateralOffset(vpos, a, BaseVehicle.GetModelInfo(model, VehicleModelInfoType.WheelsRear), part == VehiclePart.RightRearTire ? 90f : -90f));
                case VehiclePart.RightMidTire:
                case VehiclePart.LeftMidTire:
                    return WorldToLocal(vpos, a, ApplyLateralOffset(vpos, a, BaseVehicle.GetModelInfo(model, VehicleModelInfoType.WheelsMiddle), part == VehiclePart.RightMidTire ? 90f : -90f));
                case VehiclePart.RightFrontSeat:
                case VehiclePart.LeftFrontSeat:
                    return WorldToLocal(vpos, a, ApplyLateralOffset(vpos, a, BaseVehicle.GetModelInfo(model, VehicleModelInfoType.FrontSeat), part == VehiclePart.RightFrontSeat ? 90f : -90f));
                case VehiclePart.RightRearSeat:
                case VehiclePart.LeftRearSeat:
                    return WorldToLocal(vpos, a, ApplyLateralOffset(vpos, a, BaseVehicle.GetModelInfo(model, VehicleModelInfoType.RearSeat), part == VehiclePart.RightRearSeat ? 90f : -90f));
                case VehiclePart.Hood:
                    {
                        var size = BaseVehicle.GetModelInfo(model, VehicleModelInfoType.Size);
                        var wp = ApplyForwardOffset(vpos, a, size.Y / 2f, 0f);
                        var lp = WorldToLocal(vpos, a, wp);
                        var fw = BaseVehicle.GetModelInfo(model, VehicleModelInfoType.WheelsFront);
                        return new Vector3(lp.X, lp.Y - 0.2f, fw.Z + 0.5f);
                    }
                case VehiclePart.Trunk:
                    {
                        var size = BaseVehicle.GetModelInfo(model, VehicleModelInfoType.Size);
                        var wp = ApplyForwardOffset(vpos, a, size.Y / 2f, 180f);
                        var lp = WorldToLocal(vpos, a, wp);
                        var rw = BaseVehicle.GetModelInfo(model, VehicleModelInfoType.WheelsRear);
                        return new Vector3(lp.X, lp.Y + 0.3f, rw.Z + 0.5f);
                    }
                case VehiclePart.PetrolCap:
                    {
                        var info = BaseVehicle.GetModelInfo(model, VehicleModelInfoType.PetrolCap);
                        bool hasCap = GetPetrolCapSide(vehicleId) != PetrolCapSide.None;
                        return new Vector3(info.X, hasCap ? info.Y : info.Y - 2f, info.Z);
                    }
                default:
                    return Vector3.Zero;
            }
        }

        public static float GetVehiclePartAngle(int vehicleId, VehiclePart part)
        {
            var v = BaseVehicle.Find(vehicleId);
            if (v == null) return 0f;
            float a = v.Angle;
            switch (part)
            {
                case VehiclePart.FrontTire:
                case VehiclePart.Hood: return a - 180f;
                case VehiclePart.RightFrontTire:
                case VehiclePart.RightRearTire:
                case VehiclePart.RightMidTire:
                case VehiclePart.RightFrontSeat:
                case VehiclePart.RightRearSeat: return a + 90f;
                case VehiclePart.LeftFrontTire:
                case VehiclePart.LeftRearTire:
                case VehiclePart.LeftMidTire:
                case VehiclePart.LeftFrontSeat:
                case VehiclePart.LeftRearSeat: return a - 90f;
                case VehiclePart.PetrolCap:
                    {
                        var cap = GetPetrolCapSide(vehicleId);
                        if (cap == PetrolCapSide.Right) return a + 90f;
                        if (cap == PetrolCapSide.Left) return a - 90f;
                        if (cap == PetrolCapSide.Front) return a - 180f;
                        return a;
                    }
                default: return a;
            }
        }

        public static bool IsPlayerNearVehiclePart(BasePlayer player, int vehicleId, VehiclePart part, float radius = 0.5f, float offset = 0f)
        {
            if (player == null) return false;
            int modelId = (int)(BaseVehicle.Find(vehicleId)?.Model ?? 0);
            if (part == VehiclePart.PetrolCap)
            {
                var cap = GetPetrolCapSide(vehicleId);
                if (cap == PetrolCapSide.None || modelId is 557 or 486 or 592) radius += 1.5f;
                else if (modelId is 471 or 571) radius += 0.5f;
            }
            var pos = GetPosNearVehiclePart(vehicleId, part, offset);
            return player.Position.DistanceTo(pos) <= radius;
        }

        public static void SetPlayerNearVehiclePart(BasePlayer player, int vehicleId, VehiclePart part, float offset = 0.5f)
        {
            if (player == null) return;
            var pos = GetPosNearVehiclePart(vehicleId, part, offset);
            player.Position = pos;
            player.Angle = GetVehiclePartAngle(vehicleId, part);
        }

        public static PetrolCapSide GetPetrolCapSide(int vehicleId)
        {
            var v = BaseVehicle.Find(vehicleId);
            if (v == null) return PetrolCapSide.None;
            int m = (int)v.Model;
            if (m is 401 or 402 or 411 or 429 or 451 or 456 or 478 or 482 or 483 or 486 or 489 or 490 or 495 or
                496 or 505 or 506 or 514 or 518 or 524 or 533 or 541 or 546 or 554 or 557 or
                560 or 561 or 562 or 565 or 566 or 579 or 580 or 585 or 589 or 599 or 600 or
                602 or 603) return PetrolCapSide.Right;
            if (m is 400 or 403 or 404 or 405 or 408 or 409 or 410 or 413 or 414 or 415 or 416 or
                418 or 419 or 420 or 421 or 422 or 423 or 426 or 427 or 431 or 433 or 434 or
                436 or 437 or 438 or 439 or 440 or 442 or 443 or 445 or 448 or 455 or 458 or
                459 or 461 or 462 or 463 or 467 or 468 or 470 or 471 or 475 or 477 or 479 or
                480 or 485 or 491 or 492 or 494 or 498 or 499 or 500 or 502 or 503 or 504 or
                507 or 508 or 515 or 516 or 517 or 521 or 522 or 523 or 525 or 526 or 527 or
                528 or 529 or 534 or 535 or 536 or 540 or 542 or 543 or 544 or 547 or 549 or
                550 or 551 or 552 or 555 or 558 or 559 or 568 or 571 or 573 or 574 or 578 or
                581 or 582 or 583 or 586 or 587 or 588 or 596 or 597 or 598 or 601 or 605 or
                609) return PetrolCapSide.Left;
            if (m is 407 or 412 or 428 or 444 or 466 or 545 or 567 or 572 or 575 or 576 or 604)
                return PetrolCapSide.Back;
            if (m == 424) return PetrolCapSide.Front;
            return PetrolCapSide.None;
        }

        public static string GetVehiclePartName(VehiclePart part) => part switch
        {
            VehiclePart.FrontTire => "Front Tire",
            VehiclePart.RearTire => "Rear Tire",
            VehiclePart.RightFrontTire => "Right Front Tire",
            VehiclePart.LeftFrontTire => "Left Front Tire",
            VehiclePart.RightRearTire => "Right Rear Tire",
            VehiclePart.LeftRearTire => "Left Rear Tire",
            VehiclePart.RightMidTire => "Right Mid Tire",
            VehiclePart.LeftMidTire => "Left Mid Tire",
            VehiclePart.RightFrontSeat => "Right Front Seat",
            VehiclePart.LeftFrontSeat => "Left Front Seat",
            VehiclePart.RightRearSeat => "Right Rear Seat",
            VehiclePart.LeftRearSeat => "Left Rear Seat",
            VehiclePart.Hood => "Hood",
            VehiclePart.Trunk => "Trunk",
            VehiclePart.PetrolCap => "Petrol Cap",
            _ => "Unknown"
        };

        #region Helpers

        private static float Sin(float deg) => MathF.Sin(deg * MathF.PI / 180f);
        private static float Cos(float deg) => MathF.Cos(deg * MathF.PI / 180f);

        private static Vector3 ApplyLateralOffset(Vector3 vpos, float a, Vector3 info, float angleMod)
        {
            float px = vpos.X + info.X * Sin(-a + angleMod) + info.Y * Sin(-a);
            float py = vpos.Y + info.X * Cos(-a + angleMod) + info.Y * Cos(-a);
            return new Vector3(px, py, info.Z);
        }

        private static Vector3 ApplyForwardOffset(Vector3 vpos, float a, float half, float angleMod)
        {
            float px = vpos.X + half * Sin(-a + angleMod);
            float py = vpos.Y + half * Cos(-a + angleMod);
            return new Vector3(px, py, vpos.Z);
        }

        private static Vector3 WorldToLocal(Vector3 vpos, float a, Vector3 wp)
        {
            float dx = wp.X - vpos.X, dy = wp.Y - vpos.Y;
            float lx = dx * Cos(a) + dy * Sin(a);
            float ly = -dx * Sin(a) + dy * Cos(a);
            return new Vector3(lx, ly, wp.Z);
        }

        #endregion
    }
}