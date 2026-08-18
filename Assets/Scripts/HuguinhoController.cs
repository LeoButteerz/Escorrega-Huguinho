using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class HuguinhoController : MonoBehaviour
{
    [Header("Movement Configs")]
    public float speed = 5f;
    public float grid_size = 1f;

    [Header("Layer Masks")]
    public LayerMask obstacle_layer;
    public LayerMask ground_layer;

    [Header("State Flags")]
    public bool is_sliding = false;
    private Vector3 current_position;

    [Header("Cooldown Configs")]
    public float cooldown = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Faz com que Huguinho comece alinhado na grid
        current_position = SnapToGrid(transform.position);
        transform.position = current_position;

    }

    // Update is called once per frame
    void Update()
    {
        //inputs apenas válidos se huguinho não estiver escorregando ou andando
        if (is_sliding) return;

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
            StartCoroutine(Walk(input_direction));
        }
    }

    //co-rotina que movimenta huguinho na grid e utiliza a flag de andar
    private IEnumerator Walk(Vector3 end_position)
    {
        //tira a possibilidade de andar pra outra posição antes do movimento atual acabar
        is_sliding = true;
        bool was_stopped = false;

        //movimentação do ponto A para o ponto B
        while (true)
        {
            Vector3 target_position = current_position + (end_position * grid_size);

            if(IsObstacleInDirection(target_position))
            {
                was_stopped = true;
                break;
            }

            if (IsWin(target_position))
            {
                Debug.Log("win");
                break;
            }

            yield return StartCoroutine(MoveTowardsTarget(target_position));
            current_position = SnapToGrid(target_position);

            if (!IsOnIce(current_position))
            {
                break;
            }
        }

        if (was_stopped)
        {
            yield return new WaitForSeconds(cooldown);
        }

        is_sliding = false;
    }

    //co-rotina que desliza huguinho na última direção registrada até que ele colida com algo
    private IEnumerator MoveTowardsTarget(Vector3 target_position)
    {
        while(Vector3.Distance(transform.position, target_position) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target_position, speed * Time.deltaTime);
            yield return null;
        }

        transform.position = target_position;
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

    private bool IsObstacleInDirection(Vector3 target_position)
    {
        // Check a sphere centered inside the target tile
        Vector3 tileCenter = target_position + Vector3.up * 0.5f;
        
        // 0.4f radius covers the center of a 1x1 tile without touching adjacent tiles
        return Physics.CheckSphere(tileCenter, grid_size * 0.4f, obstacle_layer);
    }

    private bool IsOnIce(Vector3 position)
    {
        Vector3 origin = position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 1.5f, ground_layer))
        {
            return hit.collider.CompareTag("Ice");
        }

        return false;
    }

    private bool IsWin(Vector3 target_position)
    {
        Vector3 origin = current_position + Vector3.up * 0.5f;
        Vector3 dir = (target_position - current_position).normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, grid_size))
        {
            return hit.collider.CompareTag("Oto");
        }

        return false;
    }
}
