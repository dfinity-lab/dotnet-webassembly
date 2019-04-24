func fac(n : Nat) : Nat {
  if (n == 0)  {
    assert(false);
    return(1);
  }
  else {
    n *
    fac (n-1);
  };
};

let res = fac 20;
