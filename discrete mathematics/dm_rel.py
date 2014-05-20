
def show(matrix):
    # Print out matrix
    for col in matrix:
        print col
   
def zero(m, n):
    # Create zero matrix
    new_matrix = [[False for row in range(n)] for col in range(m)]
    return new_matrix

def mult(matrix1, matrix2):
    # Matrix multiplication
    if len(matrix1[0]) != len(matrix2):
        # Check matrix dimensions
        print 'Matrices must be m*n and n*p to multiply!'
    else:
        # Multiply if correct dimensions
        new_matrix = zero(len(matrix1), len(matrix2[0]))
        for i in range(len(matrix1)):
            for j in range(len(matrix2[0])):
                for k in range(len(matrix2)):
                    new_matrix[i][j] = new_matrix[i][j] or (matrix1[i][k] and matrix2[k][j])
        return new_matrix

def antisimm(matrix):
    for i in range(1, 4):
        for j in range(i + 1, 5):
            if((matrix[i][j] == 1) and (matrix[i][j] == matrix[j][i])):
                return False
    return True

def trans(m1):
    m2 = mult(m1, m1)
    if (m1 == m2):
        return True
    for i in range(1, len(m1)):
        for j in range(1, len(m1)):
            if (m1[i][j] == False and m2[i][j] == True):
                return "False (cell [%i, %i]" % (i, j)
    return "???"

def show_m(matrix):
    print "  |",
    for i in range(1, 6):
        print i,
    print
    print "-" * 15
    i = 1
    for row in matrix:
        print i, "|",
        for x in row:
            print int(x),
        print
        i = i + 1

#matrix for p
mp = []
for i in range(1, 6):
    line = []
    for j in range(1, 6):
        #formula for p
        line.append(((i + j) % 2) != 0)
    mp.append(line)

#matrix for t
mt = []
for i in range(1, 6):
    line = []
    for j in range(1, 6):
        #formula for t
        line.append((-1 <= i - j) and (i - j < 0))
    mt.append(line)

mpt = mult(mp, mt)

print "#" * 20
for x in ('mp', 'mt', 'mpt'):
    print "=" * 15
    print x
    x = eval(x)
    show_m(x)
    print "transposed:"
    mtt = []
    mtt = map(list, zip(*x))
    show_m(mtt)
    print "symmetrical?", mtt == x
    print "antisymmetrical?", antisimm(x)
    print "transitive?", trans(x)
    x2 = mult(x, x)
    print "squared:"
    show_m(x2)
print "#" * 20
