# -*- coding: utf-8 -*-

class PriorityQueue(object):
    """
    Une classe générique pour représenter une file de priorité
    """

    # Public
    def enqueue(self, item, priority):
        """
        Rajoute un élément dans la liste suivant sa priorité
        @param item   	: L'élément à rajouter
        @param priority : Sa priorité
        """
        place = self.__find_place(priority)
        self.__list.insert(place, (priority, item))
        self.__length += 1

    def dequeue(self):
        """
        Retourne le premier élément de la liste, ie. celui qui a la plus petite priorité
        @return L'élément avec la plus petite priorité
        """
        if self.__length == 0:
            raise "The queue is empty, can't dequeue"
        self.__length -= 1
        return self.__list.pop(0)[1]

    def change_priority(self, item, newPriority):
        """
        Change la priorité d'un élément
        @param item   		: L'élément à modifier
        @param newPriority 	: Sa nouvelle priorité
        """
        self.__list.pop(self.__find_item(item))
        self.__length -= 1
        self.enqueue(item, newPriority)

    def get_list_items(self):
        """
        Retourne tous les éléments de la liste
        @return Les éléments de la liste
        """
        return [value[1] for value in self.__list]

    def get_priority(self, item):
        """
        Retourne la priorité de l'élément
        @return L'élément dont on veut récupérer la priorité
        """
        return self.__list[self.__find_item(item)][0]


    # Private
    def __init__(self):
        self.__list = []
        self.__length = 0

    def __len__(self):
        return self.__length

    def __repr__(self):
        res = "Length = " + str(self.__length) + "\n"
        for item in self.__list:
            res += "Priority " + str(item[0]) + " for " + str(item[1]) + "\n"
        return res

    def __find_item(self, item):
        """Retourne l'index d'item dans la liste"""
        return [value[1] for value in self.__list].index(item)

    def __find_place(self, priority):
        """Trouve l'endroit ou ajouter une nouvelle priorité
        Recherche dichotomique"""
        min = 0
        max = self.__length - 1
        i = -1
        while max >= min:
            curr = (max + min) / 2
            if priority == self.__list[curr][0]:
                return curr
            elif priority < self.__list[curr][0]:
                max = curr - 1
            else:
                min = curr + 1
            i = min
        return i


