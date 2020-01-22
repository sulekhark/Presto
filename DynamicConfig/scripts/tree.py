#!/usr/bin/env python3

import sys

class Node(object):
    def __init__(self, data=None, parent=None):
        self.data = data
        self.parent = parent
        self.offset = []
        self.exception = []
        self.children = []

    def addChild(self, child, offset, ex=None):
        self.children.append(child)
        self.offset.append(offset)
        self.exception.append(ex)
        child.parent = self

    def getChild(self, childData):
        for n, child in enumerate(self.children):
            if childData in child.data:
                return n
        return -1


    def getAncestor(self, data):
        ancestor = self
        while ancestor.parent is not None:
            ancestor = ancestor.parent 
            if ancestor.data == data:
                return ancestor
        return None 

    def getSiblings(self):
        if self.parent is None:
            return []
        else:
            return [a for a in self.parent.children if a is not self]

    def getIndex(self):
        for n, child in enumerate(self.parent.children):
            if child is self:
                return n

    def remChild(self, index):
        try:
            self.children.pop(index)
            self.offset.pop(index)
            self.exception.pop(index)
        except IndexError:
            print('Cannot remove. Child Index not found')

    def printTree(self, level=0):
        print('    ' * level + repr(self))
        for child in self.children:
            child.printTree(level+1)

    def __repr__(self):
        return 'Node:{0}'.format(self.data)

def testTree():
    root = Node(data='root')
    child1 = Node(data='child1')
    root.addChild(child1, "0")
    
    child2 = Node(data='child2')
    child1.addChild(child2, "0")
    child1.addChild(child2, "0")
    root.addChild(child2, "0")

    print('\n---TEST PRINT TREE---\n')
    root.printTree()
    child1.printTree()

    print('\n---TEST METHODS---\n')

    print('roots children:  {0}'.format(root.children))
    print('child1 children:  {0}'.format(child1.children))
    print('root parent:  {0}'.format(root.parent))
    print('child1 parent:  {0}'.format(child1.parent))
    print('child2 parent:  {0}'.format(child2.parent))

    print('root siblings:  {0}'.format(root.getSiblings()))
    print('child1 siblings:  {0}'.format(child1.getSiblings()))

    print('child1 index:  {0}'.format(child1.getIndex()))
    print('child2 index:  {0}'.format(child2.getIndex()))

    print('\n---TEST REMOVE CHILD---\n')

    print('roots children:  {0}'.format(root.children))
    root.remChild(child2.getIndex())
    print('roots children:  {0}'.format(root.children))

# testTree()
