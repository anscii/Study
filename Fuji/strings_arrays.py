#!/bin/python

# 1.1 Are all symbols in string unique?

def sym_unique(str):
	symbols = []
	unique = True
	for e in str:
		if unique is False:
			break
		if e in symbols:
			unique = False
			break
		else:
			symbols.append(e)
	print(unique)

# 1.3 Are two strings anagrams?
def anagrams(str1, str2):
	t1 = sorted(tuple(str1))
	t2 = sorted(tuple(str2))

	res = (t1 == t2)
	print(res)

#1.4 Replace all whitespaces with '%20'

def spaces(data):
	res = ''
	for e in data:
		if e == ' ':
			res += '%20'
		else:
			res += e
	print(res)
#1.5 Zip strings with same symbols counters (aaabbc -> a3b2c). If result > original then return original
def zip_str(data):
	last = None
	cnt = 0
	res = ''

	for i in range(len(data)):
		e = data[i]

		if last is None:
			last = e
			continue
		
		if last == e:
			if cnt == 0:
				cnt = 2
			else:
				cnt+=1
		else:
			res += last
			last = e

			if cnt > 1:
				res += str(cnt)
				cnt = 1
	res += last
	if cnt > 1:
		res += str(cnt)

	print(data)

	if len(res) < len(data):
		print(res)
	else:
		print(data)

#1.7 is str2 shift of str2? Use 'in' only once

def is_shift(str1, str2):
	res = False
	if len(str1) != len(str1):
		return res

	t1 = sorted(tuple(str1))
	t2 = sorted(tuple(str2))

	if t1 != t2:
		return res

	tmp = ''

	for i in range(len(str1)):
		for j in range(len(str2)):
			if str1[i] == str2[j]:
				print(i)
				print(j)
				k = j
				while str2[k] == str1[k]:
					k+=1
				if k == len(str1) - 1:
					print (str2[j:k])


	return res



### RUN
#sym_unique("abr")
#anagrams('abra', 'bara')
#spaces('this is a test')
#zip_str('aaabbcdeeef')
res = is_shift('erbottlewat', 'waterbottle')
print(res)

