/// <summary>
/// Interface cho mọi vật thể có thể tương tác trong game.
/// Gắn lên GameObject cùng với InteractableObject component.
/// </summary>
public interface IInteractable
{
    /// <summary>Tên hiển thị trên UI khi player nhìn vào vật thể.</summary>
    string InteractName { get; }

    /// <summary>Gợi ý hành động, ví dụ: "Nhấn [E] để xem".</summary>
    string InteractHint { get; }

    /// <summary>Được gọi khi player nhấn phím tương tác (E).</summary>
    void Interact();
}
