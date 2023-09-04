from itertools import permutations, product

def safe_eval(expr):
    try:
        return eval(expr)
    except ZeroDivisionError:
        return None

if __name__ == "__main__":
    target = 257
    digits = ['6', '1', '3', '5', '1', '3']
    ops = ['+', '-', '*', '/']
    solutions = set()

    # Generate all permutations of digits
    for p in permutations(digits):
        # Generate all possible ways to split these digits into 2 to 6 numbers
        for split1 in range(1, len(p)):
            for split2 in range(split1 + 1, len(p)):
                for split3 in range(split2 + 1, len(p)):
                    for split4 in range(split3 + 1, len(p)):
                        for split5 in range(split4 + 1, len(p)):
                            nums = [int(''.join(p[:split1])), int(''.join(p[split1:split2])), int(''.join(p[split2:split3])), int(''.join(p[split3:split4])), int(''.join(p[split4:split5])), int(''.join(p[split5:]))]
                            nums = [num for num in nums if num != 0]  # Remove zeros
                        
                            # Generate all permutations of these numbers
                            for num_perm in permutations(nums):
                                # Generate all combinations of operations
                                for op_comb in product(ops, repeat=len(num_perm) - 1):
                                    expr_parts = [str(num_perm[0])]
                                    for i in range(1, len(num_perm)):
                                        expr_parts.append(op_comb[i - 1])
                                        expr_parts.append(str(num_perm[i]))
                                    expr = ''.join(expr_parts)
                                
                                    result = safe_eval(expr)
                                    if result == target:
                                        solutions.add(expr)

    print("Solutions found:")
    for solution in solutions:
        print(solution)