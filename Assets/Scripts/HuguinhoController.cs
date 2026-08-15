using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class HuguinhoController : MonoBehaviour
{
    [Header("Movement Configs")]
    public float speed = 5f;
    public float grid_size = 1f;

    private bool is_sliding = false;
    private bool was_stopped = false;
    private bool can_walk = true;
    private Vector3 target_position;
    private Vector3 last_direction;

    [Header("Cooldown Configs")]
    public float cooldown = 0.5f;
    public float timer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Faz com que Huguinho comece alinhado na grid
        target_position = SnapToGrid(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        //inputs apenas válidos se huguinho não estiver escorregando ou andando
        if (is_sliding || !can_walk) return;

        Vector3 input_direction = Vector3.zero;

        if (Keyboard.current != null)
        {
            //coloca a direção da movimentação
            if (Keyboard.current.wKey.wasPressedThisFrame) input_direction = Vector3.forward;
            else if (Keyboard.current.aKey.wasPressedThisFrame) input_direction = Vector3.left;
            else if (Keyboard.current.sKey.wasPressedThisFrame) input_direction = Vector3.back;
            else if (Keyboard.current.dKey.wasPressedThisFrame) input_direction = Vector3.right;
        }

        //se houver uma direção de movimentação, calcula para onde huguinho deve ir
        if (input_direction != Vector3.zero)
        {
            last_direction = input_direction;
            was_stopped = false;
            Vector3 next_position = target_position + (input_direction * grid_size);
            StartCoroutine(Walk(next_position));
        }
    }

    //co-rotina que movimenta huguinho na grid e utiliza a flag de andar
    private IEnumerator Walk(Vector3 end_position)
    {
        can_walk = false;
        target_position = end_position;

        while (Vector3.Distance(transform.position, end_position) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, end_position, speed * Time.deltaTime);
            yield return null;
        }

        transform.position = end_position;
        can_walk = true;
    }

    private IEnumerator Slide(Vector3 direction)
    {
        is_sliding = true;
        can_walk = false;
        was_stopped = false;

        while (!was_stopped)
        {
            transform.position = Vector3.MoveTowards(transform.position, transform.position + (direction * grid_size), speed * Time.deltaTime);
            yield return null;
        }

        transform.position = SnapToGrid(transform.position);
        is_sliding = false;
        can_walk = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ice" && !was_stopped)
        {
            IceSliding();
            Debug.Log("is sliding");
        }

        else if (collision.gameObject.tag == "Oto")
        {
            Debug.Log("win");
        }

        else if (!was_stopped)
        {
            was_stopped = true;
            Debug.Log("was stopped");
        }
    }

    //função para alinhar huguinho na grid (acho que dá pra retirar depois)
    private Vector3 SnapToGrid(Vector3 position)
    {
        return new Vector3(
            Mathf.Round(position.x/grid_size) * grid_size,
            position.y,
            Mathf.Round(position.z/grid_size) * grid_size
        );
    }

    public void IceSliding()
    {
        StopAllCoroutines();
        StartCoroutine(Slide(last_direction));
    }
}
