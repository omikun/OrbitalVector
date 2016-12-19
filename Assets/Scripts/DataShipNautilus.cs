using UnityEngine;

public class DataShipNautilus : ScriptableObject {
    int cost = 300 * 1000 * 1000;
    public float health = 200;
    float mass = 100; //Tons, dry mass
    public int GetCost() {
        return cost; //TODO sum cost of components
        //also scailer? production bonus and what not
    }
    float buildTime = 4 * 31 * 24 * 3600; //seconds
    public float GetBuildTime() {
        return buildTime;
    }
    //engine
    public class Engine {
        public int count = 2; //number of engines
        public float isp = 400; //seconds
        public float thrust = 250; //kN
        public float heatOutput = 3000; //kilowatt
        public float mass = 4;
        public float GetMass() {
            return count * mass;
        }
        public float GetThrust()
        {
            return thrust*count;
        }
        //in tons
        public float GetFlowRate()
        {
            return thrust * isp / 1000.0f;
        }
    }
    public class FuelTank {
        public int count = 1;
        public float mass = 400; //Tons
        public float GetMass() {
            return count * mass;
        }
        public void consume(float fuel)
        {
            mass -= fuel / count;
        }
    }
    //weapons
    public class Missile {
        public int count = 4;
        public Engine engine_;
        public FuelTank fuelTank_;
        public float mass = 20; //Tons
        public float GetMass() {
            var perItem = mass + engine_.GetMass() + fuelTank_.GetMass();
            return count * perItem;
        }
    }
    Engine engine_;
    Missile missile_;
    FuelTank fuelTank_;

    public float GetMass() {
        return mass + engine_.GetMass() + fuelTank_.GetMass()
            + missile_.GetMass();
    }
    public float GetAcceleration(float throttle)
    {
        //a = f / a; 
        return engine_.GetThrust() / GetMass();
    }
    public float GetDV(float dt, float throttle) {
        //get dv
        var dv = GetAcceleration(throttle) * dt;
        //consume fuel
        var fuelConsumed = dt * engine_.GetFlowRate();
        fuelTank_.consume(fuelConsumed);
        return dv;
    }
    //power generation
        //tie with throttle?
    //heat rejection
    //life support
    //sensors
}