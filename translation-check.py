import json
import sys

def func():
    if len(sys.argv) < 3:
        print('insufficient arguments < 2')
        return

    base: dict = None
    variant: dict = None

    with open(sys.argv[1], 'r', encoding='utf-8') as f:
        base = json.loads(f.read())

    with open(sys.argv[2], 'r', encoding='utf-8') as f:
        variant = json.loads(f.read())

    # check missing keys and add them
    for key in base['keys']:
        if not key in variant['keys']:
            print(f'variant was missing key "{key}", added now.')
            variant['keys'][key] = "MISSING_KEY"

    keys_to_delete = []

    # check extra keys and queue to delete them
    for key in variant['keys']:
        if not key in base['keys']:
            print(f'variant has extra key "{key}", deleted now.')
            keys_to_delete.append(key)

    # delete them
    for key in keys_to_delete:
        del variant['keys'][key]
    
    with open(sys.argv[1], 'w', encoding='utf8') as f:
        json.dump(base, f, sort_keys=True, indent=4, ensure_ascii=False)
    with open(sys.argv[2], 'w', encoding='utf8') as f:
        json.dump(variant, f, sort_keys=True, indent=4, ensure_ascii=False)

func()
