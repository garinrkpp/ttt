using Photon.Deterministic;
using Quantum;

public class LocalInput : QuantumInput
{
    public override Tuple<Input, DeterministicInputFlags> PollInput(int player)
    {
        Input i = new Input();

        //Need to query unity to get controls
        FP x = FP.FromFloat_UNSAFE(UnityEngine.Input.GetAxis("Horizontal"));
        FP y = FP.FromFloat_UNSAFE(UnityEngine.Input.GetAxis("Vertical"));

        if(player == 1)
        {
            y = -y;
        }

        i.Movement = new FPVector2(x,y);
        i.Fire = UnityEngine.Input.GetButton("Fire1");
        return Tuple.Create(i, DeterministicInputFlags.Repeatable);
    }
}
