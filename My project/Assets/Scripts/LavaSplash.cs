// ���� �̸�: LavaSplash.cs
using UnityEngine;

public class LavaSplash : MonoBehaviour
{
    // --- public ���� (�ν����� â���� ���� ������ �� �ֽ��ϴ�) ---

    [Header("���� ����")]
    [Tooltip("�Ҷ��� �ڱ�ġ�� �ּ� ��")]
    public float minJumpForce = 5f; // �Ҷ��� ��� �÷����� �ּ� ���� ũ��

    [Tooltip("�Ҷ��� �ڱ�ġ�� �ִ� ��")]
    public float maxJumpForce = 10f; // �Ҷ��� ��� �÷����� �ִ� ���� ũ��

    [Header("���� ����")]
    [Tooltip("�Ҷ˿� ����� �߷� ��")]
    public float gravity = 9.8f; // �Ҷ��� �Ʒ��� ������� �߷��� ũ��

    // --- private ���� (��ũ��Ʈ ���ο����� ���˴ϴ�) ---
    private float verticalVelocity; // �Ҷ��� ���� ���� �ӵ�
    private float initialYPosition; // �Ҷ��� ó�� ������ Y�� ��ġ

    /// <summary>
    /// ���� ������Ʈ�� ó�� ������ �� �� �� ȣ��Ǵ� �Լ��Դϴ�.
    /// </summary>
    void Start()
    {
        // ���� ������Ʈ�� ���� Y�� ��ġ�� �����մϴ�.
        // �� ��ġ�� ���߿� �Ҷ��� ������� �ٽ� ���ƿԴ��� �Ǵ��ϴ� ������ �˴ϴ�.
        initialYPosition = transform.position.y;

        // �ּ� ���� �ִ� �� ���̿��� ������ ���� �����Ͽ� �ʱ� ���� �ӵ��� �����մϴ�.
        // �̷� ���� �Ҷ��� Ƣ�� ������ ���̰� �Ź� �޶����� �˴ϴ�.
        verticalVelocity = Random.Range(minJumpForce, maxJumpForce);
    }

    /// <summary>
    /// �� �����Ӹ��� ȣ��Ǵ� �Լ��Դϴ�.
    /// </summary>
    void Update()
    {
        // �߷� ���� ���� �ӵ��� ����ؼ� ���ݴϴ�. (�Ʒ��� �������� ȿ��)
        // Time.deltaTime�� �����ִ� ������ ��� ��ǻ�Ϳ��� ������ �ӵ��� �����̰� �ϱ� �����Դϴ�.
        verticalVelocity -= gravity * Time.deltaTime;

        // ���� ���� �ӵ���ŭ Y������ ������Ʈ�� �̵���ŵ�ϴ�.
        transform.Translate(Vector3.up * verticalVelocity * Time.deltaTime);

        // �Ҷ��� ���� Y�� ��ġ�� ó�� ��ġ���� ���ų� ��������
        if (transform.position.y <= initialYPosition)
        {
            // �Ҷ��� ������� �ٽ� ���ƿ� ������ �����ϰ� ������Ʈ�� �ı�(����)�մϴ�.
            // �̷��� ���� ������ ������ ����ɼ��� ���ʿ��� ������Ʈ�� ��� �׿� ������ ���ϵ� �� �ֽ��ϴ�.
            Destroy(gameObject);
        }
    }
}
