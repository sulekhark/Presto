(set-option :auto-config true)
(set-option :produce-models true)

(set-option :mbqi true)
(declare-fun f (Int) Int)
(declare-fun p (Int) Bool)
(declare-fun p2 (Int) Bool)
(declare-const a Int)
(declare-const b Int)
(declare-const c Int)
(assert (forall ((x Int)) 
                (=> (not (p x)) (= (f x) (+ x 1)))))
(assert (forall ((x Int)) 
                (=> (and (p x) (not (p2 x))) (= (f x) x))))
(assert (forall ((x Int)) 
                (=> (p2 x) (= (f x) (- x 1)))))
(assert (p b))
(assert (p c))
(assert (p2 a))
(assert (> (f a) b))
(check-sat)
(get-model)
