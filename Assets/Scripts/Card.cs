using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    Animator _animator;

    [SerializeField] [Range(0, 2)] float maxDelayTime = 0.25f;
    [SerializeField] Sprite spriteShirt;
    [SerializeField] Sprite[] icons;

    public static int iconsLength = 51;
    bool _iconSide;
    int _id;
    int _nubmer;

    Image _image;
    Button _button;
    Sprite _spriteIcon;

    public int Nubmer { get { return _nubmer; }  set { _nubmer = value; } }
    public int Id { get { return _id; }  set { _id = value; } }
    public int Icon { set { _spriteIcon = icons[value]; } }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _image = GetComponent<Image>();
        _button = GetComponent<Button>();
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(Random.Range(0, maxDelayTime));

        _animator.SetTrigger("fadeOut");
    }

    public void Click()
    {
        _button.interactable = false;
        GameManager.inctance.LocalPlayer.Interactable(false);
        GameManager.inctance.InteractWithCard(this);
    }

    public void Flip()
    {
        _animator.SetTrigger("flip");
    }

    public void Dropped()
    {
        --CardContainer.inctance.CardAmount;
        _animator.SetTrigger("fadeIn");

        if (CardContainer.inctance.CardAmount == 0)
        {
            GameManager.inctance.EndGame();
        }
    }

    public int SwapSprite()
    {
        _iconSide = !_iconSide;
        _button.interactable = !_iconSide;
        
        if (_iconSide)
        {
            _image.sprite = _spriteIcon;
        }
        else
        {
            _image.sprite = spriteShirt;
        }

        transform.GetChild(0).gameObject.SetActive(_iconSide);

        return _id;
    }
}
