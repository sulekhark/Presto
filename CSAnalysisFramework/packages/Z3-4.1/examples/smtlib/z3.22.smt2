(set-option :auto-config true)
(set-option :produce-models true)

(define-sort Set (T) (Array T Bool))
(declare-const a (Set Int))
(declare-const b (Set Int))
(declare-const c (Set Int))
(declare-const x Int)
(push)
(assert (not (= ((_ map and) a b) ((_ map not) ((_ map or) ((_ map not) b) ((_ map not) a))))))
(check-sat)
(pop)
(push) 
(assert (and (select ((_ map and) a b) x) (not (select a x))))
(check-sat)
(pop)
(push) 
(assert (and (select ((_ map or) a b) x) (not (select a x))))
(check-sat)
(get-model)
(assert (and (not (select b x))))
(check-sat)
