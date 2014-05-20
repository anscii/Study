def xor(x1,x2,x3, x4 = 0, x5 = 0, x6 = 0, x7 = 0):
    return bool(x1) ^ bool(x2) ^ bool(x3) ^ bool(x4) ^ bool(x5) ^ bool(x6) ^ bool(x7)

def get_res(vect,x1,x2,x3):
    n = int((str(int(x1))+str(int(x2))+str(int(x3))), 2)
    return int(vect[n])

# (x2 NAND (x2 OR x3)) AND (x2 NOR (NOT x3)) OR (x1 XOR x3)
def funcf(x1, x2, x3):
    return ((not (x2 and (x2 or x3))) and (not(x2 or (not x3))) or ((x1 or x3) and (not(x1 and x3))))
def funcw(x1, x2, x3):
    #return ((not(x1) and x2 and not(x3)) or (x1 and not(x2) and not(x3)) or (x1 and x2 and x3))
    vect = "00101001"
    return get_res(vect,x1,x2,x3)

def funcg(x1,x2,x3):
    vect = "11001011"
    return get_res(vect,x1,x2,x3)

def sam(fnc):
    for x1 in (0, 1):
        for x2 in (0,1):
            for x3 in (0,1):
                f = fnc(x1,x2,x3)
                fn = fnc(not(x1),not(x2),not(x3))
                if (f == fn):
                    print "x1, x2, x3: %s, %s, %s" % (x1, x2, x3)
                    print "f = ",f,"fn =",fn
                    return False
    return True

def lin(fnc):
    a0 = fnc(0,0,0)
    a3 = xor1(fnc(0,0,1), a0)
    a2 = xor1(fnc(0,1,0), a0)
    a1 = xor1(fnc(1,0,0), a0)
    a23 = xor1(fnc(0,1,1), xor(a2,a3,a0))
    a13 = xor1(fnc(1,0,1), (xor(a1,a3,a0)))
    a12 = xor1(fnc(1,1,0), (xor(a1,a2,a0)))
    a123 = xor1(fnc(1,1,1), (xor(a12, a23, a13, a1, a2, a3, a0)))
    print "a0 = %i" % a0
    print "a3 = %i" % a3
    print "a2 = %i" % a2
    print "a1 = %i" % a1
    print "a23 = %i" % a23
    print "a13 = %i" % a13
    print "a12 = %i" % a12
    print "a123 = %i" % a123
    
    res = ""
    is_lin = True

    if (a123):
        res += "x1x2x3 @ "
        is_lin = False
    if (a23):
        res += "x2x3 @ "
        is_lin = False
    if (a13):
        res += "x1x3 @ "
        is_lin = False
    if (a12):
        res += "x1x2 @ "
        is_lin = False
    if (a1):
        res += "x1 @"
    if (a2):
        res += "x2 @"
    if (a3):
        res += "x3 @"
    if (a0):
        res += " 1"
    else:
        res = res[:-1]

    print res #"f(x1,x2,x3) = %i x1x2x3@ %i x1x2@ %i x2x3@ %i x1x3@ %i x1@ %i x2@ %i x3@ %i" % (a123,a12,a23,a13,a1,a2,a3,a0)
    return is_lin
# res = arg1 @ arg2
# #returns arg1
def xor1(res, arg2):
    #res = arg1 XOR arg2    
        xor = (False != bool(arg2))
        if (xor == res):
            return 0
        else:
            return 1

def show(fnc):
    print "x1|x2|x3|f"
    for x1 in (0,1):
        for x2 in (0,1):
            for x3 in (0,1):
                print x1, x2, x3, "|", int(fnc(x1, x2, x3))

print "x1x2x3|fwg"
for x1 in (0,1):
    for x2 in (0,1):
        for x3 in (0,1):
            print x1, x2, x3, "|", int(funcf(x1, x2, x3)), int(funcw(x1, x2, x3)), int(funcg(x1, x2, x3))

for item in (funcf, funcw, funcg):
    print "### %s ###" % item
    print "T0?", item(0,0,0) == 0
    print "T1?", item(1,1,1) == 1
    print "S?", sam(item)
    print "lin"
    print lin(item)
